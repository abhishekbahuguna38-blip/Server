# IIS Setup Script for AdminServer
# Run this script as Administrator

param(
    [string]$Port = "5030",
    [string]$SiteName = "AdminServer",
    [string]$AppPoolName = "AdminServerPool",
    [string]$PhysicalPath = "C:\inetpub\AdminServer"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "IIS Setup Script for AdminServer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Step 1: Enable IIS Features
Write-Host "[1/8] Enabling IIS Features..." -ForegroundColor Yellow
try {
    $features = @(
        "IIS-WebServerRole",
        "IIS-WebServer",
        "IIS-CommonHttpFeatures",
        "IIS-HttpErrors",
        "IIS-ApplicationDevelopment",
        "IIS-NetFxExtensibility45",
        "IIS-HealthAndDiagnostics",
        "IIS-HttpLogging",
        "IIS-Security",
        "IIS-RequestFiltering",
        "IIS-Performance",
        "IIS-WebServerManagementTools",
        "IIS-ManagementConsole",
        "IIS-StaticContent",
        "IIS-DefaultDocument",
        "IIS-DirectoryBrowsing",
        "IIS-WebSockets",
        "IIS-ApplicationInit",
        "IIS-ISAPIExtensions",
        "IIS-ISAPIFilter",
        "IIS-HttpCompressionStatic"
    )
    
    foreach ($feature in $features) {
        $state = Get-WindowsOptionalFeature -Online -FeatureName $feature -ErrorAction SilentlyContinue
        if ($state -and $state.State -ne "Enabled") {
            Write-Host "  Installing $feature..." -ForegroundColor Gray
            Enable-WindowsOptionalFeature -Online -FeatureName $feature -All -NoRestart | Out-Null
        }
    }
    Write-Host "  Success: IIS Features enabled" -ForegroundColor Green
}
catch {
    Write-Host "  Error: Failed to enable IIS features: $_" -ForegroundColor Red
    exit 1
}

# Step 2: Check .NET 8 Hosting Bundle
Write-Host "`n[2/8] Checking .NET 8 Hosting Bundle..." -ForegroundColor Yellow
try {
    $dotnetRuntimes = dotnet --list-runtimes 2>$null | Select-String "Microsoft.AspNetCore.App 8"
    if ($dotnetRuntimes) {
        Write-Host "  Success: .NET 8 Runtime found" -ForegroundColor Green
    }
    else {
        Write-Host "  Error: .NET 8 Hosting Bundle NOT found!" -ForegroundColor Red
        Write-Host "  Please download and install from:" -ForegroundColor Yellow
        Write-Host "  https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "  Error: Failed to check .NET runtime: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Build Application
Write-Host "`n[3/8] Building Application..." -ForegroundColor Yellow
try {
    $projectPath = Join-Path $PSScriptRoot "AdminServerStub\AdminServerStub.csproj"
    if (-not (Test-Path $projectPath)) {
        Write-Host "  Error: Project file not found: $projectPath" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "  Building and publishing to $PhysicalPath..." -ForegroundColor Gray
    dotnet publish $projectPath -c Release -o $PhysicalPath --nologo
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Success: Application built successfully" -ForegroundColor Green
    }
    else {
        Write-Host "  Error: Build failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "  Error: Failed to build application: $_" -ForegroundColor Red
    exit 1
}

# Step 4: Import IIS Module
Write-Host "`n[4/8] Loading IIS Module..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration -ErrorAction Stop
    Write-Host "  Success: IIS Module loaded" -ForegroundColor Green
}
catch {
    Write-Host "  Error: Failed to load IIS module: $_" -ForegroundColor Red
    exit 1
}

# Step 5: Create Application Pool
Write-Host "`n[5/8] Creating Application Pool..." -ForegroundColor Yellow
try {
    if (Test-Path "IIS:\AppPools\$AppPoolName") {
        Write-Host "  Application pool '$AppPoolName' already exists. Removing..." -ForegroundColor Gray
        Remove-WebAppPool -Name $AppPoolName
    }
    
    New-WebAppPool -Name $AppPoolName | Out-Null
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "managedRuntimeVersion" -Value ""
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "autoStart" -Value $true
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
    
    Write-Host "  Success: Application pool '$AppPoolName' created" -ForegroundColor Green
}
catch {
    Write-Host "  Error: Failed to create application pool: $_" -ForegroundColor Red
    exit 1
}

# Step 6: Create Website
Write-Host "`n[6/8] Creating Website..." -ForegroundColor Yellow
try {
    if (Test-Path "IIS:\Sites\$SiteName") {
        Write-Host "  Website '$SiteName' already exists. Removing..." -ForegroundColor Gray
        Remove-Website -Name $SiteName
    }
    
    $portInUse = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if ($portInUse) {
        Write-Host "  Warning: Port $Port is already in use!" -ForegroundColor Yellow
    }
    
    New-Website -Name $SiteName -PhysicalPath $PhysicalPath -ApplicationPool $AppPoolName -Port $Port | Out-Null
    
    Write-Host "  Success: Website '$SiteName' created on port $Port" -ForegroundColor Green
}
catch {
    Write-Host "  Error: Failed to create website: $_" -ForegroundColor Red
    exit 1
}

# Step 7: Set Permissions
Write-Host "`n[7/8] Setting Folder Permissions..." -ForegroundColor Yellow
try {
    $acl = Get-Acl $PhysicalPath
    $permission = "IIS_IUSRS", "Read,ReadAndExecute,ListDirectory", "ContainerInherit,ObjectInherit", "None", "Allow"
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($rule)
    Set-Acl $PhysicalPath $acl
    
    $appPoolIdentity = "IIS AppPool\$AppPoolName"
    $acl = Get-Acl $PhysicalPath
    $permission = $appPoolIdentity, "Read,ReadAndExecute,ListDirectory", "ContainerInherit,ObjectInherit", "None", "Allow"
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($rule)
    Set-Acl $PhysicalPath $acl
    
    $logsPath = Join-Path $PhysicalPath "logs"
    if (-not (Test-Path $logsPath)) {
        New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
    }
    
    $acl = Get-Acl $logsPath
    $permission = $appPoolIdentity, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($rule)
    Set-Acl $logsPath $acl
    
    Write-Host "  Success: Permissions configured" -ForegroundColor Green
}
catch {
    Write-Host "  Error: Failed to set permissions: $_" -ForegroundColor Red
    exit 1
}

# Step 8: Configure Firewall
Write-Host "`n[8/8] Configuring Firewall..." -ForegroundColor Yellow
try {
    $ruleName = "AdminServer HTTP Port $Port"
    $existingRule = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
    
    if ($existingRule) {
        Write-Host "  Firewall rule already exists. Updating..." -ForegroundColor Gray
        Remove-NetFirewallRule -DisplayName $ruleName
    }
    
    New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Protocol TCP -LocalPort $Port -Action Allow | Out-Null
    
    Write-Host "  Success: Firewall configured for port $Port" -ForegroundColor Green
}
catch {
    Write-Host "  Warning: Failed to configure firewall: $_" -ForegroundColor Red
    Write-Host "  You may need to configure firewall manually" -ForegroundColor Yellow
}

# Security Hardening
Write-Host "`n[Security] Applying Security Settings..." -ForegroundColor Yellow
try {
    Set-WebConfigurationProperty -PSPath "IIS:\Sites\$SiteName" -Filter "system.webServer/directoryBrowse" -Name "enabled" -Value $false
    Set-WebConfigurationProperty -PSPath "IIS:\Sites\$SiteName" -Filter "system.webServer/security/requestFiltering/requestLimits" -Name "maxAllowedContentLength" -Value 30000000
    Write-Host "  Success: Security settings applied" -ForegroundColor Green
}
catch {
    Write-Host "  Warning: Some security settings failed to apply" -ForegroundColor Yellow
}

# Start the site
Write-Host "`n[Final] Starting Website..." -ForegroundColor Yellow
try {
    Start-Website -Name $SiteName
    Start-WebAppPool -Name $AppPoolName
    Start-Sleep -Seconds 2
    
    $siteState = (Get-Website -Name $SiteName).State
    $poolState = (Get-WebAppPoolState -Name $AppPoolName).Value
    
    if ($siteState -eq "Started" -and $poolState -eq "Started") {
        Write-Host "  Success: Website is running!" -ForegroundColor Green
    }
    else {
        Write-Host "  Warning: Website state: $siteState, App Pool state: $poolState" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "  Error: Failed to start website: $_" -ForegroundColor Red
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Deployment Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your server is now accessible at:" -ForegroundColor White
Write-Host "  Local:   http://localhost:$Port" -ForegroundColor Cyan
Write-Host "  Network: http://$(hostname):$Port" -ForegroundColor Cyan
Write-Host "  Swagger: http://localhost:$Port/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Installation Details:" -ForegroundColor White
Write-Host "  Site Name:     $SiteName" -ForegroundColor Gray
Write-Host "  App Pool:      $AppPoolName" -ForegroundColor Gray
Write-Host "  Physical Path: $PhysicalPath" -ForegroundColor Gray
Write-Host "  Port:          $Port" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to open the site in your browser..." -ForegroundColor Green
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Start-Process "http://localhost:$Port/swagger"
