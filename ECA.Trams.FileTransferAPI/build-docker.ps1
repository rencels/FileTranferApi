# Builds the API image without running dotnet restore inside Docker.
# Use this on ECA networks where Docker cannot reach api.nuget.org (NU1301).
param(
    [string]$ImageTag = "trams-filetransfer-api:latest",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$scriptRoot = $PSScriptRoot
$projectPath = Join-Path $scriptRoot "ECA.Trams.FileTransferAPI\ECA.Trams.FileTransferAPI.csproj"
$publishPath = Join-Path $scriptRoot "artifacts\publish"
$dockerfilePath = Join-Path $scriptRoot "ECA.Trams.FileTransferAPI\Dockerfile.runtime"

Write-Host "Publishing application to $publishPath ..."
if (Test-Path $publishPath) {
    Remove-Item $publishPath -Recurse -Force
}

dotnet publish $projectPath -c $Configuration -o $publishPath
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "Building Docker image $ImageTag ..."
docker build -t $ImageTag -f $dockerfilePath $scriptRoot
if ($LASTEXITCODE -ne 0) {
    throw "docker build failed with exit code $LASTEXITCODE"
}

Write-Host "Done. Image: $ImageTag"
