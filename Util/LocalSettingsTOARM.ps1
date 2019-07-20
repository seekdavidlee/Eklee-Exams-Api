param(
    [string]$LocalSettingsFilePath, 
    [string]$armTemplate,
    [string]$organization,
    [string]$project,
    [string]$username,
    [securestring]$password)

$content = Get-Content -Path $LocalSettingsFilePath | ConvertFrom-Json

$global:gl = @()
$global:pl = @()

$ignoreKeywords = @("Values:", "IsEncrypted", "Host:")
function ProcessValue {
    param ($p, $key)

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
        Write-Host "Assert" $propertyName -ForegroundColor Green

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

ReadObj -obj $content -key $null


$content = Get-Content -Path $armTemplate | ConvertFrom-Json

$appSettings = [PSCustomObject] @{ }

$global:gl | ForEach-Object {
    $_.Name
    $_.Value
    $appSettings | Add-Member -MemberType NoteProperty -Name $_.Name -Value $_.Value
}

if (!$content.variables.appSettings) {    
    $content.variables | Add-Member -MemberType NoteProperty -Name "appSettings" -Value $appSettings
    Write-Host "AppSettings property is added." -ForegroundColor Green
}
else {
    Write-Host "AppSettings is there."
    $content.variables.appSettings = $appSettings
    Write-Host "AppSettings property is updated." -ForegroundColor Green
}

$yamlOut = ""
$global:pl | ForEach-Object {
    $content.parameters | Add-Member -MemberType NoteProperty -Name $_ -Value @{"defaultValue" = ""; "type" = "String" } -Force
    $yamlOut += "-${_} `$(Deploy.${_}) "
}

$content.resources | ForEach-Object {
    
    if ($_.type -eq "Microsoft.Web/sites/config") {

        if (!$_.properties.numberOfWorkers) {
            $_.properties = "[variables('appSettings')]"
        }        
    }

    if ($_.type -eq "Microsoft.Web/sites/slots/config") {
        $_.properties = "[variables('appSettings')]"
    }
}

($content | ConvertTo-Json -depth 50).Replace("\u0027", "'") | Out-File $armTemplate -Encoding UTF8

Write-Host $yamlOut

$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
$uPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

$bytes = [System.Text.ASCIIEncoding]::ASCII.GetBytes("${username}:${uPassword}")
$encodedText = [Convert]::ToBase64String($bytes)

$url = "https://dev.azure.com/$organization/$project/_apis/distributedtask/variablegroups?api-version=5.0-preview.1"

$autHeaders = @{"Authorization" = "Basic $encodedText"; }
$payloads = Invoke-RestMethod -Method Get -Uri $url -Headers $autHeaders -ContentType "application/json"

$updateSettings = $null
$payloads.Value | ForEach-Object {
    if ($_.Name -eq "Deploy") {
        $updateSettings = $_        
    }
}

if ($updateSettings -ne $null) {
    
    $variables = New-Object -TypeName psobject

    $global:gl | ForEach-Object {
        if ($_.AsDeployParameter) {
            $parameterName = "Deploy." + $_.Name.Replace(":", "_")
            $isSecret = ($parameterName.EndsWith("Secret") -or $parameterName.EndsWith("Key")).ToString()
            $variables | Add-Member -MemberType NoteProperty -Name $parameterName -Value @{"Value" = $_.OriginalValue; "IsSecret" = $isSecret; } -ErrorAction Stop   
        }     
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
else {
    Write-Host "Cannot find appsettings." -ForegroundColor Yellow
}

