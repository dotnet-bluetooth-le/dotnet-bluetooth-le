@echo off
"../Source/.nuget/nuget.exe" install FAKE -Version 4.10.5

"FAKE.4.10.5/tools/FAKE.exe" build.fsx %*