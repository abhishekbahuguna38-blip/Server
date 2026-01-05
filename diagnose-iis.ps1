# Diagnostic Script for IIS and ASP.NET Core Module
# Run as Administrator

Write-Host "=== IIS Diagnostic Tool ===" -ForegroundColor Cyan
Write-Host ""

# Check if admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: Must run as Administrator!" -ForegroundColor Red
    exit 1
}

# Check ASP.NET Core Module DLL
Write-Host "[1] Checking ASP.NET Core Module..." -ForegroundColor Yellow
$aspNetCoreModule = "C:\Windows\System32\inetsrv\aspnetcorev2.dll"
if (Test-Path $aspNetCoreModule) {
    Write-Host "  Found: $aspNetCoreModule" -ForegroundColor Green
    $version = (Get-Item $aspNetCoreModule).VersionInfo.FileVersion
    Write-Host "  Version: $version" -ForegroundColor Green
} else {
    Write-Host "  NOT FOUND: $aspNetCoreModule" -ForegroundColor Red
    Write-Host "  This is the problem! ASP.NET Core Module is not installed." -ForegroundColor Red
}

# Check if module is registered in IIS
Write-Host "`n[2] Checking IIS Module Registration..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration -ErrorAction Stop
    $modules = Get-WebGlobalModule
    $aspNetCoreModules = $modules | Where-Object { $_.Name -like "*AspNetCore*" }
    
    if ($aspNetCoreModules) {
        Write-Host "  ASP.NET Core Module is registered:" -ForegroundColor Green
        $aspNetCoreModules | ForEach-Object { Write-Host "    - $($_.Name)" -ForegroundColor Green }
    } else {
        Write-Host "  ASP.NET Core Module is NOT registered in IIS" -ForegroundColor Red
    }
} catch {
    Write-Host "  Error checking modules: $_" -ForegroundColor Red
}

# Check .NET runtimes
Write-Host "`n[3] Checking .NET Runtimes..." -ForegroundColor Yellow
$runtimes = dotnet --list-runtimes | Select-String "Microsoft.AspNetCore.App"
if ($runtimes) {
    Write-Host "  Found ASP.NET Core runtimes:" -ForegroundColor Green
    $runtimes | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
} else {
    Write-Host "  No ASP.NET Core runtimes found" -ForegroundColor Red
}

# Check web.config
Write-Host "`n[4] Checking web.config..." -ForegroundColor Yellow
$webConfig = "C:\inetpub\AdminServer\web.config"
if (Test-Path $webConfig) {
    Write-Host "  Found: $webConfig" -ForegroundColor Green
    try {
        [xml]$xml = Get-Content $webConfig
        Write-Host "  XML is valid" -ForegroundColor Green
    } catch {
        Write-Host "  XML is INVALID: $_" -ForegroundColor Red
    }
} else {
    Write-Host "  NOT FOUND: $webConfig" -ForegroundColor Red
}

# Solution
Write-Host "`n=== SOLUTION ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "If ASP.NET Core Module is missing, you need to:" -ForegroundColor Yellow
Write-Host "1. Download .NET 8 Hosting Bundle from:" -ForegroundColor White
Write-Host "   https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Look for: 'Hosting Bundle' (NOT just Runtime)" -ForegroundColor White
Write-Host "   File: dotnet-hosting-8.0.x-win.exe" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Run the installer with REPAIR option if already installed" -ForegroundColor White
Write-Host ""
Write-Host "4. After installation, run:" -ForegroundColor White
Write-Host "   iisreset" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to open download page..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Start-Process "https://dotnet.microsoft.com/download/dotnet/8.0"
