$StackName = $env:stackName
	
$url = "https://" + $StackName + "-staging.azurewebsites.net"
@{
    values = @(
        @{ key = "endpoint"; value = "$url"; },
        @{ key = "client_secret"; value = "$env:Postman_client_secret"; },
        @{ key = "client_id"; value = "$env:Postman_client_id"; },
        @{ key = "resource_id"; value = "$env:Postman_resource_id"; },
        @{ key = "tenant_id"; value = "$env:Postman_tenant_id"; },
        @{ key = "isRemote"; value = "$env:Postman_isRemote"; },
        @{ key = "tenant1Id"; value = "$env:Postman_tenant1Id"; },
        @{ key = "tenant1user1"; value = "$env:Postman_tenant1user1"; },
        @{ key = "tenant1user1password"; value = "$env:Postman_tenant1user1password"; },
        @{ key = "resourceId"; value = "$env:Postman_resourceId"; },
        @{ key = "clientId"; value = "$env:Postman_clientId"; },
        @{ key = "clientSecret"; value = "$env:Postman_clientSecret"; },
        @{ key = "adminUser1"; value = "$env:Postman_adminUser1"; },
        @{ key = "adminUser1password"; value = "$env:Postman_adminUser1password"; },
        @{ key = "adminResourceId"; value = "$env:Postman_adminResourceId"; },
        @{ key = "adminId"; value = "$env:Postman_adminId"; },
        @{ key = "adminClientId"; value = "$env:Postman_adminClientId"; },
        @{ key = "adminClientSecret"; value = "$env:Postman_adminClientSecret"; }
    )
} | ConvertTo-Json -depth 100 | Out-File -encoding ASCII postman_environment.json

npm install --save-dev newman

Write-Host "Running postman"

node_modules\.bin\newman run tests\Eklee.Exam.Api.postman_collection.json -e postman_environment.json --reporters cli,junit --reporter-junit-export $env:Common_TestResultsDirectory\report.xml