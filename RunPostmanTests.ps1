$Tenant1Id = $env:tenant1Id
$Tenant1user1 = $env:tenant1user1
$Tenant1user1password = $env:tenant1user1password
$ResourceId = $env:resourceId
$ClientId = $env:clientId 
$ClientSecret = $env:clientSecret
$StackName = $env:stackName
	
	  $url = "https://" + $StackName + "-staging.azurewebsites.net"
      @{
        values=@(
        @{
            key="endpoint";
            value="$url";
        }, 
        @{
            key="tenant1Id";
            value="$Tenant1Id";
        }, 
        @{
            key="clientId";
            value="$ClientId";
        }, 
        @{
            key="clientSecret";
            value="$ClientSecret";
        }, 
        @{
            key="tenant1user1";
            value="$Tenant1user1";
        }, 
        @{
            key="tenant1user1password";
            value="$Tenant1user1password";
        }, 
        @{
            key="resourceId";
            value="$ResourceId";
        })
      } | ConvertTo-Json -depth 100 | Out-File -encoding ASCII postman_environment.json
      npm install
      Write-Host "Running postman tests."
	  node_modules\.bin\newman run tests\Eklee.Exam.Api.postman_collection.json -e postman_environment.json --reporters junit --reporter-junit-export $env:Common_TestResultsDirectory\report.xml