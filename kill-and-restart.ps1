# Kill all AdminServer instances and restart cleanly
Write-Host "Stopping all AdminServer instances..." -ForegroundColor Yellow

# Kill all dotnet processes related to AdminServer
Get-Process dotnet -ErrorAction SilentlyContinue | ForEach-Object {
    try {
        $processPath = $_.Path
        if ($processPath -like "*AdminServer*") {
            Write-Host "Killing process $($_.Id) - $processPath" -ForegroundColor Red
            Stop-Process -Id $_.Id -Force
        }
    } catch {
        # Ignore errors
    }
}

# Also kill by working directory
Get-Process dotnet -ErrorAction SilentlyContinue | ForEach-Object {
    try {
        $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)").CommandLine
        if ($cmdLine -like "*AdminServer*") {
            Write-Host "Killing process $($_.Id)" -ForegroundColor Red
            Stop-Process -Id $_.Id -Force
        }
    } catch {
        # Ignore errors
    }
}

Write-Host ""
Write-Host "Waiting 3 seconds..." -ForegroundColor Cyan
Start-Sleep -Seconds 3

Write-Host ""
Write-Host "Starting AdminServer on port 5030..." -ForegroundColor Green
Write-Host "Server will be accessible at: http://localhost:5030" -ForegroundColor Cyan
Write-Host "Swagger UI: http://localhost:5030/swagger" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Set environment to Development
$env:ASPNETCORE_ENVIRONMENT="Development"

# Navigate to project directory and run
Set-Location -Path "$PSScriptRoot\AdminServerStub"
dotnet run
