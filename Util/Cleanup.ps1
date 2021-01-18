param(
	[Parameter(Mandatory = $True)][string]$Name)

$resources = az resource list --tag stackName=$Name| ConvertFrom-Json

$funcId = ($resources | Where-Object { $_.type -eq "Microsoft.Web/sites" }).id

az resource delete --ids $funcId

$resources | Where-Object { $_.type -ne "Microsoft.Web/sites" } | ForEach-Object { az resource delete --ids $_.id }