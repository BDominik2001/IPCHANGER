<#
.SYNOPSIS
    Az IP Changer self-contained kiadásának elkészítése és a Windows telepítő
    (setup.exe) legenerálása Inno Setuppal.

.DESCRIPTION
    1) `dotnet publish` – self-contained, win-x64 build a ../publish mappába.
    2) Inno Setup (ISCC.exe) – a ../dist mappába rakja a IpChanger-Setup-<verzió>.exe-t.

    Előfeltételek:
      - .NET 8 SDK          https://dotnet.microsoft.com/download/dotnet/8.0
      - Inno Setup 6        https://jrsoftware.org/isdl.php

.EXAMPLE
    ./build-installer.ps1
    ./build-installer.ps1 -Version 1.2.0 -Runtime win-arm64
#>
[CmdletBinding()]
param(
    [string]$Version = "1.3.0",
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot          # a repó gyökere
$project = Join-Path $root "src/IpChanger/IpChanger.csproj"
$publishDir = Join-Path $root "publish"
$issFile = Join-Path $PSScriptRoot "IpChanger.iss"

Write-Host "==> Tisztítás: $publishDir" -ForegroundColor Cyan
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

Write-Host "==> dotnet publish ($Runtime, self-contained)" -ForegroundColor Cyan
dotnet publish $project `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:Version=$Version `
    -o $publishDir

# Inno Setup fordító megkeresése
$iscc = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
if (-not $iscc) {
    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )
    $iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}
if (-not $iscc) {
    throw "Nem található az ISCC.exe. Telepítsd az Inno Setup 6-ot: https://jrsoftware.org/isdl.php"
}

Write-Host "==> Inno Setup fordítás ($Version)" -ForegroundColor Cyan
& $iscc "/DMyAppVersion=$Version" $issFile

Write-Host "==> Kész. A telepítő itt található: $(Join-Path $root 'dist')" -ForegroundColor Green
