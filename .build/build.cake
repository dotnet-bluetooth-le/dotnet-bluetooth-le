#addin nuget:?package=Cake.Git&version=1.0.1
#addin nuget:?package=Cake.FileHelpers&version=4.0.0

using Path = System.IO.Path;
using System.Xml.Linq;
using System.Xml;

var target = Argument("target", "Build");

var NuGetTargetDir = MakeAbsolute(Directory("./nuget"));
var BuildTargetDir = MakeAbsolute(Directory("./out/lib"));
var ProjectSources = MakeAbsolute(Directory("../Source"));
var NuspecFiles = new [] { "Plugin.BLE.nuspec", "MvvmCross.Plugin.BLE.nuspec" };

string GetProjectPath(string pathPrefix, string projectName)
{
    return ProjectSources.Combine(pathPrefix).Combine(projectName).CombineWithFilePath(projectName + ".csproj").FullPath;
}

void BuildProject(string pathPrefix, string projectName, string targetSubDir)
{
    Information("Building {0} ...", projectName);
    var project = GetProjectPath(pathPrefix, projectName);
    var outputDir = BuildTargetDir.Combine(targetSubDir);
    MSBuild(project, settings => settings
            .SetConfiguration("Release")
            .WithTarget("Build")
            .UseToolVersion(MSBuildToolVersion.VS2019)
            .SetMSBuildPlatform(MSBuildPlatform.x86)
            .WithProperty("OutDir", outputDir.FullPath));
}

// string NuVersionGet (string specFile)
// {
//     var doc = System.Xml.Linq.XDocument.Load(specFile);
//     var versionElements = doc.Descendants(XName.Get("version", doc.Root.Name.NamespaceName));
//     return versionElements.First().Value;
// }

// void NuVersionSet (string specFile, string version)
// {
//     var xmlDocument = System.Xml.Linq.XDocument.Load(specFile);
//     var nsmgr = new XmlNamespaceManager(new XmlNameTable());
//     nsmgr.AddNamespace("ns", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");
//     var node = xmlDocument.Document.SelectSingleNode("//ns:version", nsmgr);
//     node.InnerText = version;
//     xmlDocument.Save(specFile);
// }

Task("Restore")
    .Does(() =>
{
    var solutions = GetFiles("../Source/*.sln");
    // Restore all NuGet packages.
    foreach(var solution in solutions)
    {
        Information("Restoring {0}", solution);
        NuGetRestore(solution);
    }
});

Task("BuildLibs")
    .Does(() =>
{
    BuildProject(".", "Plugin.BLE.Abstractions", "netstandard2.0");
    BuildProject(".", "Plugin.BLE", "netstandard2.0");
    BuildProject(".", "Plugin.BLE.Android", "android");
    BuildProject(".", "Plugin.BLE.iOS", "ios");
    BuildProject(".", "Plugin.BLE.macOS", "macOS");

    BuildProject(".", "MvvmCross.Plugins.BLE", Path.Combine("mvx","netstandard2.0"));
    BuildProject(".", "MvvmCross.Plugins.BLE.Droid", Path.Combine("mvx", "android"));
    BuildProject(".", "MvvmCross.Plugins.BLE.iOS", Path.Combine("mvx","ios"));
    BuildProject(".", "MvvmCross.Plugins.BLE.macOS", Path.Combine("mvx","macOS"));
});

Task("BuildClients")
    .Does(() =>
{
  BuildProject("BLE.Client", "BLE.Client", Path.Combine("clients", "netstandard2.0"));
  BuildProject("BLE.Client", "BLE.Client.Droid", Path.Combine("clients", "android"));
  BuildProject("BLE.Client", "BLE.Client.iOS", Path.Combine("clients", "ios"));
  BuildProject("BLE.Client", "BLE.Client.macOS", Path.Combine("clients", "macOS"));
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("BuildLibs")
    .Does(() => {});

Task("Clean").Does (() =>
{
    if (DirectoryExists (BuildTargetDir))
        DeleteDirectory (BuildTargetDir, new DeleteDirectorySettings {Recursive = true});

    CleanDirectories ("../**/bin");
    CleanDirectories ("../**/obj");
});

// ./build.ps1 -Target UpdateVersion -newVersion="2.0.1"
Task("UpdateVersion")
   .Does(() => {
    var version = Argument<string>("newVersion", "");
    var cleanVersion = version; //TODO

    if(string.IsNullOrEmpty(cleanVersion))
    {
        throw new ArgumentNullException(nameof(version));
    }

    ReplaceRegexInFiles("./**/AssemblyInfo.cs", "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))", cleanVersion);
    ReplaceRegexInFiles("./**/*.nuspec", "(?<=<version>)(.+?)(?=</version>)", cleanVersion);
    ReplaceRegexInFiles("./**/*.nuspec", "(?<=<dependency id=\"Plugin.BLE\" version=\")(.+?)(?=\" />)", cleanVersion);

});

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
    {
        foreach(var nuspec in NuspecFiles)
        {
            NuGetPack(nuspec, new NuGetPackSettings()
            {
                OutputDirectory = NuGetTargetDir,
                WorkingDirectory = BuildTargetDir,
                NoWorkingDirectory = false
            });
        }
    });

Task("Publish")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        var packages = new [] { GetFiles("nuget/Plugin.BLE*.nupkg").LastOrDefault(), GetFiles("nuget/MvvmCross*.nupkg").LastOrDefault() };

		foreach(var nupack in packages)
        {
            Information($"Pushing package: {nupack.FullPath}");
		    NuGetPush(nupack.FullPath, new NuGetPushSettings(){ Source = "https://nuget.org" });
        }
    });

RunTarget(target);
