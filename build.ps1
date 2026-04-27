# Builds the plugin and packages it as a .lplug4 file.
# Usage: powershell -ExecutionPolicy Bypass -File build.ps1 [-Version "1.0.0"]
param([string]$Version = "1.0.0")

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot
$StagingDir  = Join-Path $ProjectRoot "staging"
$OutFile     = Join-Path $ProjectRoot "ElgatoKeyLight_$($Version.Replace('.','_')).lplug4"

# Locate dotnet — prefer PATH, fall back to default install location
$DotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
$Dotnet = if ($DotnetCmd) { $DotnetCmd.Source } else { "C:\Program Files\dotnet\dotnet.exe" }
if (-not (Test-Path $Dotnet)) { throw "dotnet not found. Install .NET SDK from https://dot.net/download" }

# Always start from a clean staging dir
if (Test-Path $StagingDir) { Remove-Item $StagingDir -Recurse -Force }

Write-Host "Building ElgatoKeyLight v$Version..."

# 1. Publish
& $Dotnet publish "$ProjectRoot\ElgatoKeyLight.csproj" `
    -c Release -r win-x64 --self-contained false `
    -o "$StagingDir\win"

# 2. Stage metadata — copy contents (not the folder itself) to avoid nesting
New-Item -ItemType Directory -Path "$StagingDir\metadata" | Out-Null
Copy-Item "$ProjectRoot\metadata\*" "$StagingDir\metadata\" -Recurse -Force

# Copy plugin icon into metadata for installer display
Copy-Item "$ProjectRoot\images\PluginIcon256x256.png" "$StagingDir\metadata\Icon256x256.png" -Force

# Update version in manifest
(Get-Content "$StagingDir\metadata\LoupedeckPackage.yaml") `
    -replace '^version: .*', "version: $Version" |
    Set-Content "$StagingDir\metadata\LoupedeckPackage.yaml"

# 3. Remove files that must not be bundled
Remove-Item "$StagingDir\win\PluginApi.dll"  -ErrorAction SilentlyContinue
Remove-Item "$StagingDir\win\*.pdb"          -ErrorAction SilentlyContinue

# 4. Zip staging into .lplug4 (.lplug4 is a renamed zip)
Add-Type -AssemblyName System.IO.Compression.FileSystem
$ZipFile = [System.IO.Path]::ChangeExtension($OutFile, ".zip")
if (Test-Path $OutFile) { Remove-Item $OutFile }
if (Test-Path $ZipFile) { Remove-Item $ZipFile }
Compress-Archive -Path "$StagingDir\*" -DestinationPath $ZipFile
Rename-Item $ZipFile $OutFile
Remove-Item $StagingDir -Recurse -Force

Write-Host "Packaged: $OutFile"
