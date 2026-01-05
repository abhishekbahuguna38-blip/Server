# Restart AdminServer Script
Write-Host "Stopping AdminServer..." -ForegroundColor Yellow

# Kill any running dotnet processes in AdminServer directory
Get-Process dotnet -ErrorAction SilentlyContinue | ForEach-Object {
    if ($_.Path -like "*AdminServer*") {
        Stop-Process -Id $_.Id -Force
        Write-Host "Stopped process $($_.Id)" -ForegroundColor Green
    }
}

Start-Sleep -Seconds 2

Write-Host "Starting AdminServer..." -ForegroundColor Green
Set-Location -Path "$PSScriptRoot\AdminServerStub"
dotnet run
