# Setup AdminServer to auto-start on Windows boot
Write-Host "Setting up AdminServer to start automatically on boot..." -ForegroundColor Green

# Get the startup folder path
$startupFolder = [Environment]::GetFolderPath('Startup')
$shortcutPath = Join-Path $startupFolder "AdminServer.lnk"

# Create shortcut
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($shortcutPath)
$Shortcut.TargetPath = "C:\Users\ASUS\Desktop\AdminServer\start-server-autostart.vbs"
$Shortcut.WorkingDirectory = "C:\Users\ASUS\Desktop\AdminServer"
$Shortcut.Description = "AdminServer Auto-Start"
$Shortcut.Save()

Write-Host ""
Write-Host "✓ Auto-start configured successfully!" -ForegroundColor Green
Write-Host "✓ Server will now start automatically when you boot your PC" -ForegroundColor Green
Write-Host ""
Write-Host "Shortcut created at: $shortcutPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "To disable auto-start, simply delete the shortcut from:" -ForegroundColor Yellow
Write-Host "$startupFolder" -ForegroundColor Yellow
