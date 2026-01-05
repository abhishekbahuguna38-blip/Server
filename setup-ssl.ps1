# SSL Setup Script for AdminServer
# Run this script as Administrator AFTER running setup-iis.ps1

param(
    [string]$SiteName = "AdminServer",
    [string]$HttpsPort = "5031",
    [string]$DnsName = "localhost"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SSL Setup Script for AdminServer" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    exit 1
}

# Check if site exists
if (-not (Test-Path "IIS:\Sites\$SiteName")) {
    Write-Host "ERROR: Website '$SiteName' not found!" -ForegroundColor Red
    Write-Host "Please run setup-iis.ps1 first." -ForegroundColor Yellow
    exit 1
}

Write-Host "[1/4] Creating Self-Signed Certificate..." -ForegroundColor Yellow
try {
    # Check if certificate already exists
    $existingCert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { $_.Subject -eq "CN=$DnsName" } | Select-Object -First 1
    
    if ($existingCert) {
        Write-Host "  Certificate already exists. Using existing certificate..." -ForegroundColor Gray
        $cert = $existingCert
    } else {
        # Create new self-signed certificate
        $cert = New-SelfSignedCertificate `
            -DnsName $DnsName `
            -CertStoreLocation "Cert:\LocalMachine\My" `
            -FriendlyName "AdminServer SSL Certificate" `
            -NotAfter (Get-Date).AddYears(5)
        
        Write-Host "  ✓ Certificate created" -ForegroundColor Green
    }
    
    $certThumbprint = $cert.Thumbprint
    Write-Host "  Certificate Thumbprint: $certThumbprint" -ForegroundColor Gray
} catch {
    Write-Host "  ✗ Failed to create certificate: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n[2/4] Adding HTTPS Binding to IIS..." -ForegroundColor Yellow
try {
    Import-Module WebAdministration
    
    # Remove existing HTTPS binding if present
    $existingBinding = Get-WebBinding -Name $SiteName -Protocol "https" -Port $HttpsPort -ErrorAction SilentlyContinue
    if ($existingBinding) {
        Write-Host "  Removing existing HTTPS binding..." -ForegroundColor Gray
        Remove-WebBinding -Name $SiteName -Protocol "https" -Port $HttpsPort
    }
    
    # Add new HTTPS binding
    New-WebBinding -Name $SiteName -Protocol "https" -Port $HttpsPort -SslFlags 0
    
    Write-Host "  ✓ HTTPS binding added on port $HttpsPort" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Failed to add HTTPS binding: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n[3/4] Binding SSL Certificate..." -ForegroundColor Yellow
try {
    # Bind certificate to port using netsh
    $guid = [guid]::NewGuid().ToString("B")
    
    # Remove existing SSL binding if present
    $existingSslBinding = netsh http show sslcert ipport=0.0.0.0:$HttpsPort 2>$null
    if ($existingSslBinding -match "Certificate Hash") {
        Write-Host "  Removing existing SSL certificate binding..." -ForegroundColor Gray
        netsh http delete sslcert ipport=0.0.0.0:$HttpsPort | Out-Null
    }
    
    # Add new SSL binding
    $result = netsh http add sslcert ipport=0.0.0.0:$HttpsPort certhash=$certThumbprint appid="$guid" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ SSL certificate bound successfully" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ Warning: SSL binding may have failed" -ForegroundColor Yellow
        Write-Host "  Result: $result" -ForegroundColor Gray
    }
    
    # Also bind to IIS site
    $binding = Get-WebBinding -Name $SiteName -Protocol "https" -Port $HttpsPort
    $binding.AddSslCertificate($certThumbprint, "My")
    
} catch {
    Write-Host "  ✗ Failed to bind certificate: $_" -ForegroundColor Red
    Write-Host "  You may need to bind the certificate manually in IIS Manager" -ForegroundColor Yellow
}

Write-Host "`n[4/4] Configuring Firewall..." -ForegroundColor Yellow
try {
    $ruleName = "AdminServer HTTPS Port $HttpsPort"
    $existingRule = Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue
    
    if ($existingRule) {
        Write-Host "  Firewall rule already exists. Updating..." -ForegroundColor Gray
        Remove-NetFirewallRule -DisplayName $ruleName
    }
    
    New-NetFirewallRule -DisplayName $ruleName `
        -Direction Inbound `
        -Protocol TCP `
        -LocalPort $HttpsPort `
        -Action Allow | Out-Null
    
    Write-Host "  ✓ Firewall configured for port $HttpsPort" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Failed to configure firewall: $_" -ForegroundColor Red
}

# Restart site
Write-Host "`n[Final] Restarting Website..." -ForegroundColor Yellow
try {
    Restart-WebAppPool -Name "AdminServerPool"
    Stop-Website -Name $SiteName
    Start-Sleep -Seconds 2
    Start-Website -Name $SiteName
    
    Write-Host "  ✓ Website restarted" -ForegroundColor Green
} catch {
    Write-Host "  ⚠ Warning: Failed to restart website" -ForegroundColor Yellow
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SSL Configuration Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your server now supports HTTPS:" -ForegroundColor White
Write-Host "  • HTTPS: https://localhost:$HttpsPort" -ForegroundColor Cyan
Write-Host "  • Swagger: https://localhost:$HttpsPort/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠ IMPORTANT - Self-Signed Certificate Warning:" -ForegroundColor Yellow
Write-Host "  Your browser will show a security warning because this is" -ForegroundColor White
Write-Host "  a self-signed certificate. This is NORMAL for development." -ForegroundColor White
Write-Host ""
Write-Host "To bypass the warning:" -ForegroundColor Yellow
Write-Host "  • Chrome/Edge: Click 'Advanced' → 'Proceed to localhost'" -ForegroundColor White
Write-Host "  • Firefox: Click 'Advanced' → 'Accept the Risk and Continue'" -ForegroundColor White
Write-Host ""
Write-Host "For Production Use:" -ForegroundColor Yellow
Write-Host "  • Get a real SSL certificate from:" -ForegroundColor White
Write-Host "    - Let's Encrypt (free) - Use Win-ACME tool" -ForegroundColor Gray
Write-Host "    - Commercial CA (Sectigo, DigiCert, etc.)" -ForegroundColor Gray
Write-Host ""
Write-Host "Certificate Details:" -ForegroundColor White
Write-Host "  • Thumbprint: $certThumbprint" -ForegroundColor Gray
Write-Host "  • DNS Name:   $DnsName" -ForegroundColor Gray
Write-Host "  • Valid For:  5 years" -ForegroundColor Gray
Write-Host "  • Location:   Cert:\LocalMachine\My" -ForegroundColor Gray
Write-Host ""
Write-Host "To Trust This Certificate (Optional for Development):" -ForegroundColor Yellow
Write-Host "  1. Run: certlm.msc" -ForegroundColor White
Write-Host "  2. Navigate to: Personal → Certificates" -ForegroundColor White
Write-Host "  3. Find certificate with thumbprint: $certThumbprint" -ForegroundColor White
Write-Host "  4. Copy it to: Trusted Root Certification Authorities → Certificates" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to open the HTTPS site in your browser..." -ForegroundColor Green
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Start-Process "https://localhost:$HttpsPort/swagger"
