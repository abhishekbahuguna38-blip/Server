# AdminServer Startup Script
# Double-click this file or run: .\start-server.ps1

Write-Host "Starting AdminServer in Development Mode..." -ForegroundColor Green
Write-Host "Server will be accessible at: http://localhost:5030" -ForegroundColor Cyan
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

# Set environment to Development (allows all CORS)
$env:ASPNETCORE_ENVIRONMENT="Development"

# Navigate to project directory and run
Set-Location -Path $PSScriptRoot\AdminServerStub
dotnet run
