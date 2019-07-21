param(
    [string]$LocalSettingsFilePath, 
    [string]$LocalPostmanEnvironmentFilePath, 
    [string]$armTemplate,
    [string]$organization,
    [string]$project,
    [string]$username,
    [securestring]$password)

function ProcessValue {
    param ($p, $key)
    
    $ignoreKeywords = @("Values:", "IsEncrypted", "Host:")

    $any = $ignoreKeywords | Where-Object { $key.StartsWith($_) }

    if ($any.Count -eq 0) {

        $value = ""

        if ($key.EndsWith("Search:ServiceName")) {
            $value = "[variables('stackName')]";
        }

        if ($key.EndsWith("Search:ApiKey")) {
            $value = "[listAdminKeys(resourceId('Microsoft.Search/searchServices',variables('stackName')), '2015-08-19').PrimaryKey]";
        }

        if ($key.EndsWith("DocumentDb:Key")) {
            $value = "[listKeys(resourceId('Microsoft.DocumentDB/databaseAccounts', variables('stackName')), providers('Microsoft.DocumentDB', 'databaseAccounts').apiVersions[0]).primaryMasterKey]";
        }

        if ($key.EndsWith("DocumentDb:Url")) {
            $value = "[reference(resourceId('Microsoft.DocumentDB/databaseAccounts',variables('stackName')),'2015-04-08').documentEndpoint]"
        }

        if ($key.EndsWith("DocumentDb:RequestUnits")) {
            $value = "400"
        }

        $asDeployParameter = ($value -eq "")

        if ($asDeployParameter) {
            $parameterName = $key.Replace(":", "_")
            $value = "[parameters('$parameterName')]"
            $global:pl += $parameterName
        }

        $global:gl += @{ "Name" = $key; "Value" = $value; "OriginalValue" = $p.Value; "AsDeployParameter" = $asDeployParameter }        
    }    
}

function ReadObj {
    param ([System.Object]$obj, $key)

    foreach ($p in $obj.PsObject.Properties) {

        $propertyName = $p.Name

        if ($p.Value -is [array]) {
            
            Write-Host $propertyName "is array"

            $counter = 0
            foreach ($item in $p.Value) {

                Write-Host "Counter: $counter"
            
                if ($null -eq $key) {
                    $currentKey = "${propertyName}:${counter}"
                }
                else {
                    $currentKey = "${key}:${propertyName}:${counter}"
                }
                
                ReadObj -obj $item -key $currentKey
                                
                $counter += 1
            }
        }
        else {

            if ($null -eq $key) {
                $currentKey = $propertyName
            }
            else {
                $currentKey = "${key}:${propertyName}"
            }

            if ($p.Value -is [string]) {

                ProcessValue -p $p -key $currentKey
                continue
            }

            if ($p.Value -is [bool]) {
                ProcessValue -p $p -key $currentKey
                continue
            }

            ReadObj -obj $p.Value -key $currentKey            
        }
    }
}

function UpdateDevOpsLibrary {
    param (
        $organization,
        $project,
        $variableGroupName,
        $variables,
        $username,
        [SecureString]$password
    )

    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
    $uPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

    $bytes = [System.Text.ASCIIEncoding]::ASCII.GetBytes("${username}:${uPassword}")
    $encodedText = [Convert]::ToBase64String($bytes)

    $url = "https://dev.azure.com/$organization/$project/_apis/distributedtask/variablegroups?api-version=5.0-preview.1"

    $autHeaders = @{"Authorization" = "Basic $encodedText"; }
    $payloads = Invoke-RestMethod -Method Get -Uri $url -Headers $autHeaders -ContentType "application/json"

    $updateSettings = $null
    $payloads.Value | ForEach-Object {
        if ($_.Name -eq $variableGroupName) {
            $updateSettings = $_        
        }
    }
    if ($null -eq $updateSettings) {
        return
    }

    $id = $updateSettings.Id
    $url = "https://dev.azure.com/$organization/$project/_apis/distributedtask/variablegroups/" + $id + "?api-version=5.0-preview.1"
    $obj = @{
        "Type"        = $updateSettings.Type;
        "Name"        = $updateSettings.Name;
        "Description" = "Updated"
        "Variables"   = $variables
    }

    $body = ($obj | ConvertTo-Json -depth 50).Replace("\u0027", "'")
    Invoke-RestMethod -Method Put -Uri $url -Headers $autHeaders -ContentType "application/json" -Body $body -ErrorAction Stop
}

$postmanContent = Get-Content -Path $LocalPostmanEnvironmentFilePath | ConvertFrom-Json

$variables = New-Object -TypeName psobject

$postmanBody = ""
$envOutput = ""
$postmanContent.Values | ForEach-Object {
    $name = $_.Key
    $parameterName = "Postman." + $name.Replace(":", "_")
    $isSecret = ($parameterName.ToLower().EndsWith("secret") -or $parameterName.ToLower().EndsWith("password")).ToString()
    $variables | Add-Member -MemberType NoteProperty -Name $parameterName -Value @{"Value" = $_.Value; "IsSecret" = $isSecret; } -ErrorAction Stop
    $envName = "Postman_" + $name.Replace(":", "_")

    if ($isSecret -eq "True") {
        $envOutput += "$name`: `$($envName)`n"
    }   
    $postmanBody += "@{ key=""$name""; value=""`$env:$envName""; },`n"
}

Write-Host $envOutput

Write-Host $postmanBody

UpdateDevOpsLibrary -organization $organization -project $project `
    -variableGroupName "Postman" `
    -variables $variables `
    -username $username `
    -password $password

$content = Get-Content -Path $LocalSettingsFilePath | ConvertFrom-Json

$global:gl = @()
$global:pl = @()

ReadObj -obj $content -key $null

$content = Get-Content -Path $armTemplate | ConvertFrom-Json

$appSettings = [PSCustomObject] @{ }

$global:gl | ForEach-Object {
    $appSettings | Add-Member -MemberType NoteProperty -Name $_.Name -Value $_.Value
}

$yamlOut = ""
$global:pl | ForEach-Object {
    $content.parameters | Add-Member -MemberType NoteProperty -Name $_ -Value @{"defaultValue" = ""; "type" = "String" } -Force
    $yamlOut += "-${_} ""`$(Deploy.${_})"" "
}

$content.resources | ForEach-Object {
    
    if ($_.type -eq "Microsoft.Web/sites/config") {

        if (!$_.properties.numberOfWorkers) {
            $_.properties = $appSettings
        }        
    }

    if ($_.type -eq "Microsoft.Web/sites/slots/config") {
        $_.properties = $appSettings 
    }
}

($content | ConvertTo-Json -depth 50).Replace("\u0027", "'") | Out-File $armTemplate -Encoding UTF8

Write-Host $yamlOut

$variables = New-Object -TypeName psobject

$global:gl | ForEach-Object {
    if ($_.AsDeployParameter) {
        $parameterName = "Deploy." + $_.Name.Replace(":", "_")
        $isSecret = ($parameterName.EndsWith("Secret") -or $parameterName.EndsWith("Key")).ToString()
        $variables | Add-Member -MemberType NoteProperty -Name $parameterName -Value @{"Value" = $_.OriginalValue; "IsSecret" = $isSecret; } -ErrorAction Stop   
    }     
}

UpdateDevOpsLibrary -organization $organization -project $project `
    -variableGroupName "Deploy" `
    -variables $variables `
    -username $username `
    -password $password