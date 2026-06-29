# Deploy ECA.Trams.FileTransferAPI to Azure App Service (Linux, .NET 8 zip deploy).
# Prerequisites: az login
# Avoids Docker push and ACR build issues on ECA corporate networks.
param(
    [string]$ResourceGroup = "rg-trams-filetransfer-dev",
    [string]$Location = "westeurope",
    [string]$AppServicePlan = "asp-trams-filetransfer",
    [string]$WebAppName = "trams-filetransfer-$((Get-Random -Maximum 99999).ToString('00000'))"
)

$ErrorActionPreference = "Stop"

$az = "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"
if (-not (Test-Path $az)) {
    throw "Azure CLI not found. Install from https://learn.microsoft.com/cli/azure/install-azure-cli-windows"
}

function Set-CorporateSslForAzureCli {
    $bundlePath = Join-Path $env:TEMP "azure-cli-ca-bundle-$([Guid]::NewGuid().ToString('N')).pem"
    $certifiPath = "C:\Program Files\Microsoft SDKs\Azure\CLI2\Lib\site-packages\certifi\cacert.pem"
    if (-not (Test-Path $certifiPath)) {
        Write-Warning "Could not find Azure CLI cert bundle. Upload steps may fail behind Zscaler."
        return
    }

    $content = Get-Content $certifiPath -Raw
    $seen = @{}

    foreach ($store in @("Cert:\CurrentUser\Root", "Cert:\LocalMachine\Root")) {
        Get-ChildItem $store -ErrorAction SilentlyContinue |
            Where-Object { $_.Subject -match "Zscaler Root|ECA-PRIVATE-ROOT" } |
            ForEach-Object {
                if ($seen.ContainsKey($_.Thumbprint)) { return }
                $seen[$_.Thumbprint] = $true
                $content += "`n-----BEGIN CERTIFICATE-----`n"
                $content += [System.Convert]::ToBase64String($_.RawData, [System.Base64FormattingOptions]::InsertLineBreaks)
                $content += "`n-----END CERTIFICATE-----`n"
            }
    }

    Set-Content -Path $bundlePath -Value $content -Encoding ascii -NoNewline
    $env:REQUESTS_CA_BUNDLE = $bundlePath
    $env:SSL_CERT_FILE = $bundlePath
    Write-Host "Configured Azure CLI SSL bundle for corporate proxy."
}

# Zscaler on ECA networks can still break Kudu/ACR uploads even with a custom CA bundle.
$env:AZURE_CLI_DISABLE_CONNECTION_VERIFICATION = "true"

function Invoke-Az {
    param([string[]]$AzArguments)
    $quotedArgs = ($AzArguments | ForEach-Object {
        if ($_ -match '[\s|&<>^"]') {
            '"' + ($_ -replace '"', '""') + '"'
        } else {
            $_
        }
    }) -join ' '
    $commandLine = "`"$az`" $quotedArgs"
    cmd.exe /c $commandLine | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "az $($AzArguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

function Get-AzValue {
    param([string[]]$AzArguments)
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "SilentlyContinue"
    try {
        $value = & $az @AzArguments 2>$null
        if ($LASTEXITCODE -ne 0 -or -not $value) {
            return ""
        }
        return "$value".Trim()
    }
    finally {
        $ErrorActionPreference = $previousErrorAction
    }
}

Set-CorporateSslForAzureCli

$scriptRoot = $PSScriptRoot
$projectPath = Join-Path $scriptRoot "ECA.Trams.FileTransferAPI\ECA.Trams.FileTransferAPI.csproj"
$publishPath = Join-Path $env:TEMP "trams-filetransfer-publish-$([Guid]::NewGuid().ToString('N'))"
$zipPath = Join-Path $env:TEMP "trams-filetransfer-deploy.zip"

Write-Host "Checking Azure login..."
& $az account show -o none 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "Not logged in to Azure. Run: az login"
}

$subscription = Get-AzValue @("account", "show", "--query", "name", "-o", "tsv")
Write-Host "Using subscription: $subscription"

Write-Host "Publishing application to $publishPath ..."
if (Test-Path $publishPath) {
    Remove-Item $publishPath -Recurse -Force
}
dotnet publish $projectPath -c Release -o $publishPath
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Start-Sleep -Seconds 2

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

$tar = Get-Command tar.exe -ErrorAction SilentlyContinue
if ($tar) {
    & tar.exe -a -c -f $zipPath -C $publishPath .
    if ($LASTEXITCODE -ne 0) {
        throw "tar failed to create deployment package at $zipPath"
    }
} else {
    Compress-Archive -Path (Join-Path $publishPath "*") -DestinationPath $zipPath
}
if (-not (Test-Path $zipPath)) {
    throw "Failed to create deployment package at $zipPath"
}
Remove-Item $publishPath -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Creating resource group $ResourceGroup in $Location ..."
if ((Get-AzValue @("group", "exists", "--name", $ResourceGroup, "-o", "tsv")) -ne "true") {
    Invoke-Az @("group", "create", "--name", $ResourceGroup, "--location", $Location)
} else {
    Write-Host "Resource group already exists."
}

if (-not (Get-AzValue @("appservice", "plan", "show", "--name", $AppServicePlan, "--resource-group", $ResourceGroup, "--query", "name", "-o", "tsv"))) {
    Write-Host "Creating App Service plan $AppServicePlan ..."
    Invoke-Az @(
        "appservice", "plan", "create",
        "--name", $AppServicePlan,
        "--resource-group", $ResourceGroup,
        "--location", $Location,
        "--is-linux",
        "--sku", "B1"
    )
} else {
    Write-Host "App Service plan already exists."
}

if (-not (Get-AzValue @("webapp", "show", "--name", $WebAppName, "--resource-group", $ResourceGroup, "--query", "name", "-o", "tsv"))) {
    Write-Host "Creating Web App $WebAppName ..."
    Invoke-Az @(
        "webapp", "create",
        "--resource-group", $ResourceGroup,
        "--plan", $AppServicePlan,
        "--name", $WebAppName,
        "--runtime", "DOTNETCORE|8.0"
    )
} else {
    Write-Host "Web App already exists."
}

Write-Host "Configuring application settings..."
Invoke-Az @(
    "webapp", "config", "appsettings", "set",
    "--resource-group", $ResourceGroup,
    "--name", $WebAppName,
    "--settings",
    "ASPNETCORE_ENVIRONMENT=Production",
    "ETranslationService__OutputPath=/tmp/etranslation"
)

Write-Host "Deploying package to Azure ($zipPath) ..."
Invoke-Az @(
    "webapp", "deploy",
    "--resource-group", $ResourceGroup,
    "--name", $WebAppName,
    "--src-path", $zipPath,
    "--type", "zip",
    "--restart", "true"
)

$appUrl = "https://${WebAppName}.azurewebsites.net"
Write-Host ""
Write-Host "Deployment complete."
Write-Host "App URL:          $appUrl"
Write-Host "Deliveries URL:   $appUrl/webhook/etranslation/v1/deliveries"
Write-Host "Success URL:      $appUrl/webhook/etranslation/v1/success"
Write-Host "Error URL:        $appUrl/webhook/etranslation/v1/error"
Write-Host ""
Write-Host "Resource group:   $ResourceGroup"
Write-Host "Web App:          $WebAppName"
