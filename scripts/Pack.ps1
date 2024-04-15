$root = $PSScriptRoot -replace '\\', '/' -replace '/$', '' -replace '/scripts$', ''
$artifacts = "$root/artifacts"
$project = "$root/src/CursedQueryable/CursedQueryable.csproj"

Import-Module "$root/scripts/Exec.psm1"

if(Test-Path "$artifacts/nuget") { Remove-Item "$artifacts/nuget" -Force -Recurse }

exec {
	dotnet restore $project
}

exec {
	dotnet build $project `
	-c Release `
	/p:ContinuousIntegrationBuild=true `
	--no-restore
}

exec {
	dotnet pack $project `
	-c Release `
	-o "$artifacts/nuget" `
	--no-build 
}