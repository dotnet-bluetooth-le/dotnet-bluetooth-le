# !/bin/sh
set -x
export FVersion=4.10.5
mono --runtime=v4.0 ../Source/.nuget/nuget.exe install FAKE -Version ${FVersion}
mono --runtime=v4.0 ../Source/.nuget/nuget.exe install xunit.runner.console -Version 2.1.0
mono --runtime=v4.0 FAKE.${FVersion}/tools/FAKE.exe build.fsx $@