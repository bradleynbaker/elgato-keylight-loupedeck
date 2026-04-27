# Builds the plugin and packages it as a .lplug4 file.
# Usage: .\build.ps1 [-Version "1.0.0"]
param([string]$Version = "1.0.0")

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$StagingDir  = Join-Path $ProjectRoot "staging"
$OutFile     = Join-Path $ProjectRoot "ElgatoKeyLight_$($Version.Replace('.','_')).lplug4"

Write-Host "Building ElgatoKeyLight v$Version..."

# 1. Publish
dotnet publish "$ProjectRoot\ElgatoKeyLight.csproj" `
    -c Release -r win-x64 --self-contained false `
    -o "$StagingDir\win"

# 2. Copy metadata and images
Copy-Item "$ProjectRoot\metadata" "$StagingDir\metadata" -Recurse -Force

# Update version in manifest
(Get-Content "$StagingDir\metadata\LoupedeckPackage.yaml") `
    -replace '^version: .*', "version: $Version" |
    Set-Content "$StagingDir\metadata\LoupedeckPackage.yaml"

# 3. Remove the SDK DLL — it's provided by the host, must not be bundled
Remove-Item "$StagingDir\win\PluginApi.dll" -ErrorAction SilentlyContinue

# 4. Zip staging into .lplug4
if (Test-Path $OutFile) { Remove-Item $OutFile }
Compress-Archive -Path "$StagingDir\*" -DestinationPath "$OutFile"
Remove-Item $StagingDir -Recurse -Force

Write-Host "Packaged: $OutFile"
