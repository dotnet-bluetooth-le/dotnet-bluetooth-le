@echo off
"../Source/.nuget/nuget.exe" install FAKE -Version 4.45.2
"../Source/.nuget/nuget.exe" install xunit.runner.console -Version 2.1.0

"FAKE.4.45.2/tools/FAKE.exe" build.fsx %*