// don't forget nuget setapikey <key> before publish ;)
#r @"FAKE.4.45.2/tools/FakeLib.dll"
#r "System.Xml.Linq"

open System.Xml.Linq
open System.Xml;
open System.IO
open System.Text.RegularExpressions
open Fake
open Fake.XMLHelper;
open Fake.Git;
open Fake.NuGet.Update;


let (+/) path1 path2 = Path.Combine(path1, path2)
let RepositoryRootDir = Path.Combine("..", ".");
let NuGetTargetDir =  Path.Combine("out" ,"nuget");
let BuildTargetDir = Path.Combine("out" ,"lib");
let BootstrapFile = "BlePluginBootstrap.cs.pp"
let NugetPath =  if EnvironmentHelper.isMacOS then 
                    "mono --runtime=v4.0 ../Source/.nuget/nuget.exe" 
                 else
                    Path.Combine("..", "Source", ".nuget", "NuGet.exe");
let ProjectSources = Path.Combine("..", "Source");
let NuspecFiles = ["Plugin.BLE.nuspec"; "MvvmCross.Plugin.BLE.nuspec"];
let VanillaPluginId = "Plugin.BLE";

let Build (projectName:string, targetSubDir:string) =
    [Path.Combine(ProjectSources, projectName, projectName + ".csproj")]
     |> MSBuildRelease (BuildTargetDir +/ targetSubDir) "Build"
     |> Log "Output: "

let NuVersionGet (specFile:string) =
    let doc = System.Xml.Linq.XDocument.Load(specFile)
    let versionElements = doc.Descendants(XName.Get("version", doc.Root.Name.NamespaceName))
    (Seq.head versionElements).Value

let NuVersionVanillaDependencySet (specFile:string, version:string) = 
    let xmlDocument = new XmlDocument()
    xmlDocument.Load specFile
    let nsmgr = XmlNamespaceManager(xmlDocument.NameTable)
    nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd")
    let dependencyNodes = xmlDocument.DocumentElement.SelectNodes("//ns:dependency", nsmgr)

    let setAttributeVersion (node:XmlElement) = 
        if node.GetAttribute("id").Equals(VanillaPluginId) then
            node.SetAttribute("version", "[" + version + "]")        

    for node in dependencyNodes do
       node :?> XmlElement |> setAttributeVersion
    xmlDocument.Save specFile

let NuVersionSet (specFile:string, version:string) = 
    let xmlDocument = new XmlDocument()
    xmlDocument.Load specFile
    let nsmgr = XmlNamespaceManager(xmlDocument.NameTable)
    nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd")
    let node = xmlDocument.DocumentElement.SelectSingleNode("//ns:version", nsmgr)
    node.InnerText <- version
    xmlDocument.Save specFile


let NuPack (specFile:string, publish:bool) = 
    let version = NuVersionGet(specFile)
    let project = Path.GetFileNameWithoutExtension(specFile)

    NuGet (fun p -> 
    {p with
        ToolPath = NugetPath
        Version = version
        OutputPath = NuGetTargetDir
        WorkingDir = BuildTargetDir
        Project = project
        Publish = publish
        }) specFile

let NuPackAll (publish:bool) = 
    NuspecFiles |> List.iter (fun file -> NuPack(file, publish))

let RestorePackages() = 
    !! "../Source/**/packages.config"
    |> Seq.iter (RestorePackage (fun p ->
        { p with
            ToolPath = NugetPath
            OutputPath = Path.Combine(ProjectSources, "packages")
        }))

let RestorePackagesForSanityCheck() = 
    !! "./**/packages.config"
    |> Seq.iter (RestorePackage (fun p ->
        { p with
            ToolPath = NugetPath
            OutputPath = Path.Combine("PluginNugetTest", "packages")
            Sources = ["https://api.nuget.org/v3;"+Path.Combine(currentDirectory, "out", "nuget") ]
            Retries = 5
        }))


let UpdatePackagesForSanityCheck() = 
    !! "./**/packages.config"
    |> Seq.iter (NugetUpdate  (fun p ->
        { p with
            ToolPath = NugetPath
            Prerelease = true
            RepositoryPath = Path.Combine("PluginNugetTest", "packages")
            Sources = ["https://api.nuget.org/v3;"+Path.Combine(currentDirectory, "out", "nuget") ]
            Retries = 5
        }))


// Targets
Target "clean" (fun _ ->
    trace "cleaning up..."
    CleanDir NuGetTargetDir
    CleanDir BuildTargetDir
)

Target "build" (fun _ ->
    trace "restoring packages..."
    RestorePackages()

    trace "building libraries..."
    Build("Plugin.BLE", "pcl")
    Build("Plugin.BLE.Abstractions", "pcl")
    Build("Plugin.BLE.Android", "android")
    Build("Plugin.BLE.iOS", "ios")
    Build("MvvmCross.Plugins.BLE", Path.Combine("mvx","pcl"))
    Build("MvvmCross.Plugins.BLE.Droid", Path.Combine("mvx", "android"))
    Build("MvvmCross.Plugins.BLE.iOS", Path.Combine("mvx","ios"))
    
    trace "copy mvvm cross bootstrap files..."
    File.Copy(Path.Combine(ProjectSources, "MvvmCross.Plugins.BLE.Droid",BootstrapFile), Path.Combine(BuildTargetDir, "mvx", "android", BootstrapFile))
    File.Copy(Path.Combine(ProjectSources, "MvvmCross.Plugins.BLE.iOS", BootstrapFile), Path.Combine(BuildTargetDir, "mvx", "ios", BootstrapFile))
)


Target "sanity-check" (fun _ ->
    trace "restoring packages..."
    RestorePackagesForSanityCheck()
    UpdatePackagesForSanityCheck()
    [Path.Combine("PluginNugetTest", "PluginNugetTest.sln")]
     |> MSBuildRelease ("PluginNugetTest" +/ "testbuild") "Build"
     |> Log "Output: "
)

Target "nupack" (fun _ ->
    NuPackAll false
)

//call: build version v=1.0.0
Target "version" (fun _ ->
    let version = getBuildParam "v"
    let cleanVersion = Regex.Replace(version, @"[^\d\.].*$", "")

    BulkReplaceAssemblyInfoVersions ".." (fun info ->
    {info with
        AssemblyVersion = cleanVersion  
        AssemblyFileVersion = cleanVersion
        AssemblyInformationalVersion = version
    })

    let updateVersions(file : string, version : string) = 
        NuVersionSet(file, version)
        NuVersionVanillaDependencySet(file, version)

    NuspecFiles |> List.iter (fun file ->
    
        updateVersions(file, version)
    )
)

Target "publish" (fun _ ->    
    Run "test"

    if not (Fake.Git.Information.isCleanWorkingCopy RepositoryRootDir) then
        failwith "Uncommited changes. Please commit everything!"
    
    NuPackAll true

    let version = NuVersionGet(Seq.head NuspecFiles)    
    let branchName = Fake.Git.Information.getBranchName RepositoryRootDir
    trace ("Current GIT Branch: " + branchName)
    
    let tagName = ("v" + version)
    trace ("Creating Tag: " + tagName)
    tag RepositoryRootDir tagName
    pushTag RepositoryRootDir  "origin" tagName
)

Target "test" (fun _ ->    
    Build("Plugin.BLE.Tests", "tests")
    let result = Shell.Exec("xunit.runner.console.2.1.0" +/ "tools" +/ "xunit.console.exe", Path.Combine(BuildTargetDir, "tests", "Plugin.BLE.Tests.dll"), ".")
    if result <> 0 then failwithf "Tests failed"
)

// Dependencies
"clean"
  ==> "build"
  ==> "nupack"
  ==> "sanity-check"

"build"
  ==> "publish"

// start build
RunTargetOrDefault "build"