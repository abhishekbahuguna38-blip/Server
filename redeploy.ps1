# Redeploy script - stops IIS, rebuilds, restarts IIS
# Run as Administrator

Write-Host "=== Redeploying AdminServer ===" -ForegroundColor Cyan

# Stop app pool
Write-Host "`n[1/4] Stopping application pool..." -ForegroundColor Yellow
Import-Module WebAdministration
Stop-WebAppPool -Name "AdminServerPool"
Start-Sleep -Seconds 3
Write-Host "  App pool stopped" -ForegroundColor Green

# Build and publish
Write-Host "`n[2/4] Building and publishing..." -ForegroundColor Yellow
$projectPath = Join-Path $PSScriptRoot "AdminServerStub\AdminServerStub.csproj"
dotnet publish $projectPath -c Release -o "C:\inetpub\AdminServer" --nologo

if ($LASTEXITCODE -eq 0) {
    Write-Host "  Build successful" -ForegroundColor Green
} else {
    Write-Host "  Build failed!" -ForegroundColor Red
    exit 1
}

# Start app pool
Write-Host "`n[3/4] Starting application pool..." -ForegroundColor Yellow
Start-WebAppPool -Name "AdminServerPool"
Start-Sleep -Seconds 2
Write-Host "  App pool started" -ForegroundColor Green

# Check status
Write-Host "`n[4/4] Checking status..." -ForegroundColor Yellow
$poolState = (Get-WebAppPoolState -Name "AdminServerPool").Value
$siteState = (Get-Website -Name "AdminServer").State

if ($poolState -eq "Started" -and $siteState -eq "Started") {
    Write-Host "  Status: Running" -ForegroundColor Green
} else {
    Write-Host "  Warning: Pool=$poolState, Site=$siteState" -ForegroundColor Yellow
}

Write-Host "`n=== Deployment Complete ===" -ForegroundColor Cyan
Write-Host "Access your site at: http://localhost:5030/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press any key to open in browser..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Start-Process "http://localhost:5030/swagger"
