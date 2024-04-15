$root = $PSScriptRoot -replace '\\', '/' -replace '/$', '' -replace '/scripts$', ''
$artifacts = "$root/artifacts"
$scriptName = $MyInvocation.MyCommand.Name

Import-Module "$root/scripts/Exec.psm1"

if ([string]::IsNullOrEmpty($Env:NUGET_API_KEY)) {
	throw "${scriptName}: NUGET_API_KEY is empty or not set. Unable to push package(s)."
}

Get-ChildItem "$artifacts/nuget" -Filter "*.nupkg" | ForEach-Object {
	Write-Host "$($scriptName): Pushing $($_.Name)"
	
	exec {
		dotnet nuget push $_ `
		--source $Env:NUGET_URL `
		--api-key $Env:NUGET_API_KEY
	}
}
