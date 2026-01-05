# Enable logging and check for errors
# Run as Administrator

Write-Host "=== Enabling Logging and Checking Errors ===" -ForegroundColor Cyan

# Enable stdout logging in web.config
$webConfigPath = "C:\inetpub\AdminServer\web.config"
$webConfig = Get-Content $webConfigPath -Raw
$webConfig = $webConfig -replace 'stdoutLogEnabled="false"', 'stdoutLogEnabled="true"'
Set-Content -Path $webConfigPath -Value $webConfig
Write-Host "Logging enabled in web.config" -ForegroundColor Green

# Restart app pool
Write-Host "`nRestarting application pool..." -ForegroundColor Yellow
Import-Module WebAdministration
Restart-WebAppPool -Name "AdminServerPool"
Start-Sleep -Seconds 2

# Check Event Viewer for recent errors
Write-Host "`nChecking Event Viewer for ASP.NET Core errors..." -ForegroundColor Yellow
$errors = Get-EventLog -LogName Application -Source "IIS AspNetCore Module V2" -Newest 5 -ErrorAction SilentlyContinue
if ($errors) {
    $errors | ForEach-Object {
        Write-Host "`n--- Error at $($_.TimeGenerated) ---" -ForegroundColor Red
        Write-Host $_.Message -ForegroundColor White
    }
} else {
    Write-Host "No recent errors found in Event Viewer" -ForegroundColor Yellow
}

# Wait for log files
Write-Host "`nWaiting for log files to be created..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# Check stdout logs
$logsPath = "C:\inetpub\AdminServer\logs"
if (Test-Path $logsPath) {
    $logFiles = Get-ChildItem $logsPath -Filter "*.log" | Sort-Object LastWriteTime -Descending
    if ($logFiles) {
        Write-Host "`nLatest log file:" -ForegroundColor Green
        $latestLog = $logFiles[0]
        Write-Host $latestLog.FullName -ForegroundColor Cyan
        Write-Host "`nLog contents:" -ForegroundColor Yellow
        Get-Content $latestLog.FullName | Write-Host
    } else {
        Write-Host "No log files found yet" -ForegroundColor Yellow
    }
} else {
    Write-Host "Logs directory not found" -ForegroundColor Red
}

Write-Host "`nNow try accessing: http://localhost:5030/swagger" -ForegroundColor Cyan
Write-Host "Then check this script output for error details" -ForegroundColor Cyan
