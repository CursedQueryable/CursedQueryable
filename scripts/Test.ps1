$root = $PSScriptRoot -replace '\\', '/' -replace '/$', '' -replace '/scripts$', ''
$artifacts = "$root/artifacts"
$solution = "$root/CursedQueryable.sln"

Import-Module "$root/scripts/Exec.psm1"

if(Test-Path "$artifacts/coverage") { Remove-Item "$artifacts/coverage" -Force -Recurse }

exec {
	dotnet restore $solution
}

# Solution must be built with Debug configuration for tests to run correctly.
# This is due to DEBUG InternalsVisibleTo in Assembly.cs
exec {
	dotnet build $solution `
	-c Debug `
	--no-restore
}

exec {
	dotnet test $solution `
	/p:CollectCoverage=true `
	/p:CoverletOutput="$artifacts/coverage/" `
	/p:MergeWith="$artifacts/coverage/coverage.json" `
	/p:CoverletOutputFormat="opencover%2cjson" `
	--no-build `
	--verbosity normal
}

exec {
	reportgenerator `
	-sourcedirs:"$root/src/" `
	-targetdir:"$artifacts/coverage_report/" `
	-historydir:"$artifacts/coverage_history/" `
	-reports:"$artifacts/coverage/*.xml" `
	-reporttypes:"Html_Dark"
}