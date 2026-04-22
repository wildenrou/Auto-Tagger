param(
    [string]$Configuration = "Release",
    [string]$OutputDirectory = "dist"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $repoRoot "Jellyfin.Plugin.AutoTagger/Jellyfin.Plugin.AutoTagger.csproj"
$publishDir = Join-Path $repoRoot "Jellyfin.Plugin.AutoTagger/bin/$Configuration/net9.0"
$distDir = Join-Path $repoRoot $OutputDirectory
$packageDir = Join-Path $distDir "package"
$zipPath = Join-Path $distDir "Jellyfin.Plugin.AutoTagger.zip"
$localDotnet = Join-Path $repoRoot ".dotnet/dotnet.exe"
$dotnet = if (Test-Path $localDotnet) { $localDotnet } else { "dotnet" }

New-Item -ItemType Directory -Force -Path $distDir | Out-Null
if (Test-Path $packageDir) {
    Remove-Item -Recurse -Force $packageDir
}
New-Item -ItemType Directory -Force -Path $packageDir | Out-Null

& $dotnet build $project --configuration $Configuration

Copy-Item (Join-Path $publishDir "Jellyfin.Plugin.AutoTagger.dll") $packageDir
Copy-Item (Join-Path $repoRoot "meta.json") $packageDir

if (Test-Path $zipPath) {
    Remove-Item -Force $zipPath
}
Compress-Archive -Path (Join-Path $packageDir "*") -DestinationPath $zipPath

$md5 = (Get-FileHash -Algorithm MD5 $zipPath).Hash.ToLowerInvariant()
Set-Content -NoNewline -Path "$zipPath.md5" -Value $md5

Write-Host "Package: $zipPath"
Write-Host "MD5: $md5"
