# Builds the plugin and packages it as a .lplug4 file.
# Usage: powershell -ExecutionPolicy Bypass -File build.ps1 [-Version "1.0.0"]
param([string]$Version = "1.0.0")

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$StagingDir  = Join-Path $ProjectRoot "staging"
$OutFile     = Join-Path $ProjectRoot "ElgatoKeyLight_$($Version.Replace('.','_')).lplug4"

# Locate dotnet
$DotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
$Dotnet = if ($DotnetCmd) { $DotnetCmd.Source } else { "C:\Program Files\dotnet\dotnet.exe" }
if (-not (Test-Path $Dotnet)) { throw "dotnet not found. Install .NET SDK from https://dot.net/download" }

# Always start from a clean staging dir
if (Test-Path $StagingDir) { Remove-Item $StagingDir -Recurse -Force }
New-Item -ItemType Directory -Path $StagingDir | Out-Null

Write-Host "Building ElgatoKeyLight v$Version..."

# 1. Publish to a temp dir then copy only the DLL to staging root
#    (working plugins have DLL at root, pluginFolderWin: .)
$PublishDir = Join-Path $ProjectRoot "publish_tmp"
if (Test-Path $PublishDir) { Remove-Item $PublishDir -Recurse -Force }

& $Dotnet publish "$ProjectRoot\ElgatoKeyLight.csproj" `
    -c Release -r win-x64 --self-contained false `
    -o $PublishDir

# Copy only the plugin DLL — no .pdb, no .deps.json, no PluginApi.dll
Copy-Item "$PublishDir\ElgatoKeyLight.dll" "$StagingDir\" -Force
Remove-Item $PublishDir -Recurse -Force

# 2. Stage metadata
New-Item -ItemType Directory -Path "$StagingDir\metadata" | Out-Null
Copy-Item "$ProjectRoot\metadata\*" "$StagingDir\metadata\" -Recurse -Force
Copy-Item "$ProjectRoot\images\PluginIcon256x256.png" "$StagingDir\metadata\Icon256x256.png" -Force

# Update version in manifest
(Get-Content "$StagingDir\metadata\LoupedeckPackage.yaml") `
    -replace '^version: .*', "version: $Version" |
    Set-Content "$StagingDir\metadata\LoupedeckPackage.yaml"

# 3. Zip into .lplug4
Add-Type -AssemblyName System.IO.Compression.FileSystem
$ZipFile = [System.IO.Path]::ChangeExtension($OutFile, ".zip")
if (Test-Path $OutFile) { Remove-Item $OutFile }
if (Test-Path $ZipFile) { Remove-Item $ZipFile }
Compress-Archive -Path "$StagingDir\*" -DestinationPath $ZipFile
Rename-Item $ZipFile $OutFile
Remove-Item $StagingDir -Recurse -Force

Write-Host "Packaged: $OutFile"
