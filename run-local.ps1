# PowerShell script to run AdminServer locally

Write-Host "🚀 Starting AdminServer..." -ForegroundColor Green
Write-Host ""

# Navigate to project directory
$projectPath = Join-Path $PSScriptRoot "AdminServerStub"

# Check if dotnet is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK detected: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8.0 SDK from:" -ForegroundColor Red
    Write-Host "   https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

# Restore dependencies
Write-Host ""
Write-Host "📦 Restoring dependencies..." -ForegroundColor Cyan
dotnet restore $projectPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to restore dependencies" -ForegroundColor Red
    exit 1
}

# Build the project
Write-Host ""
Write-Host "🔨 Building project..." -ForegroundColor Cyan
dotnet build $projectPath --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}

# Run the application
Write-Host ""
Write-Host "✅ Build successful!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Starting server..." -ForegroundColor Cyan
Write-Host "   API: http://localhost:5030" -ForegroundColor Yellow
Write-Host "   Swagger: http://localhost:5030/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Gray
Write-Host ""

# Run the application
dotnet run --project $projectPath --no-build
