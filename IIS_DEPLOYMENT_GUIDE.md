# IIS Deployment Guide - Admin Server

## Prerequisites Installation

### 1. Install IIS (Internet Information Services)

**Run PowerShell as Administrator and execute:**

```powershell
# Enable IIS
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole -All

# Enable required IIS features
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpRedirect -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45 -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HealthAndDiagnostics -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-LoggingLibraries -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestMonitor -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpTracing -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Security -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Performance -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerManagementTools -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-IIS6ManagementCompatibility -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Metabase -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ManagementConsole -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-BasicAuthentication -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WindowsAuthentication -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-StaticContent -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DefaultDocument -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DirectoryBrowsing -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebSockets -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationInit -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ISAPIExtensions -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ISAPIFilter -All
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpCompressionStatic -All
```

**OR via GUI:**
1. Open **Control Panel** → **Programs** → **Turn Windows features on or off**
2. Check **Internet Information Services**
3. Expand IIS and enable:
   - Web Management Tools → IIS Management Console
   - World Wide Web Services → Application Development Features → (All)
   - World Wide Web Services → Common HTTP Features → (All)
   - World Wide Web Services → Security → (All)

### 2. Install .NET 8 Hosting Bundle

**Download and install:**
- Visit: https://dotnet.microsoft.com/download/dotnet/8.0
- Download: **ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle**
- Install and **RESTART your computer** (critical!)

**Verify installation:**
```powershell
dotnet --list-runtimes
```
You should see: `Microsoft.AspNetCore.App 8.0.x`

---

## Step 2: Build Your Application

### Option A: Using PowerShell Script (Recommended)

Run from your AdminServer directory:

```powershell
cd C:\Users\ASUS\Desktop\AdminServer

# Build for production
dotnet publish AdminServerStub/AdminServerStub.csproj -c Release -o C:\inetpub\AdminServer
```

### Option B: Using Visual Studio
1. Open `AdminServer.sln`
2. Right-click **AdminServerStub** project → **Publish**
3. Choose **Folder** → Select `C:\inetpub\AdminServer`
4. Click **Publish**

---

## Step 3: Configure IIS

### Create Application Pool

**PowerShell (Run as Administrator):**
```powershell
Import-Module WebAdministration

# Create new application pool
New-WebAppPool -Name "AdminServerPool"

# Configure for .NET (No Managed Code for .NET Core)
Set-ItemProperty IIS:\AppPools\AdminServerPool -Name "managedRuntimeVersion" -Value ""

# Set to start automatically
Set-ItemProperty IIS:\AppPools\AdminServerPool -Name "autoStart" -Value $true

# Set identity (use ApplicationPoolIdentity for security)
Set-ItemProperty IIS:\AppPools\AdminServerPool -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
```

**OR via IIS Manager GUI:**
1. Open **IIS Manager** (Windows key → type "IIS")
2. Click **Application Pools** → **Add Application Pool**
3. Name: `AdminServerPool`
4. .NET CLR version: **No Managed Code**
5. Managed pipeline mode: **Integrated**
6. Click **OK**

### Create Website

**PowerShell (Run as Administrator):**
```powershell
# Create website
New-Website -Name "AdminServer" `
    -PhysicalPath "C:\inetpub\AdminServer" `
    -ApplicationPool "AdminServerPool" `
    -Port 5030

# Set bindings
New-WebBinding -Name "AdminServer" -Protocol "http" -Port 5030 -IPAddress "*"
```

**OR via IIS Manager GUI:**
1. Right-click **Sites** → **Add Website**
2. Site name: `AdminServer`
3. Application pool: `AdminServerPool`
4. Physical path: `C:\inetpub\AdminServer`
5. Binding:
   - Type: **http**
   - IP address: **All Unassigned**
   - Port: **5030** (or any available port)
6. Click **OK**

---

## Step 4: Configure Firewall

```powershell
# Allow incoming traffic on port 5030
New-NetFirewallRule -DisplayName "AdminServer HTTP" `
    -Direction Inbound `
    -Protocol TCP `
    -LocalPort 5030 `
    -Action Allow
```

---

## Step 5: Set Folder Permissions

```powershell
# Grant IIS_IUSRS read and execute permissions
$acl = Get-Acl "C:\inetpub\AdminServer"
$permission = "IIS_IUSRS","Read,ReadAndExecute,ListDirectory","ContainerInherit,ObjectInherit","None","Allow"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($rule)
Set-Acl "C:\inetpub\AdminServer" $acl

# Grant specific app pool identity permissions (recommended for security)
$appPoolIdentity = "IIS AppPool\AdminServerPool"
$acl = Get-Acl "C:\inetpub\AdminServer"
$permission = $appPoolIdentity,"Read,ReadAndExecute,ListDirectory","ContainerInherit,ObjectInherit","None","Allow"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($rule)
Set-Acl "C:\inetpub\AdminServer" $acl
```

---

## Step 6: Create web.config (if not exists)

The file should be generated automatically, but if missing, create it:

**Location:** `C:\inetpub\AdminServer\web.config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\AdminServerStub.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="inprocess" />
      
      <!-- Enable WebSockets for SignalR -->
      <webSocket enabled="true" />
    </system.webServer>
  </location>
</configuration>
```

**Create logs directory:**
```powershell
New-Item -ItemType Directory -Path "C:\inetpub\AdminServer\logs" -Force
```

---

## Step 7: Security Hardening

### A. Configure HTTPS (Recommended)

**Using Self-Signed Certificate (Development):**
```powershell
# Create self-signed certificate
$cert = New-SelfSignedCertificate -DnsName "localhost" -CertStoreLocation "cert:\LocalMachine\My"

# Bind to IIS
New-WebBinding -Name "AdminServer" -Protocol "https" -Port 5031 -SslFlags 0

# Get the certificate thumbprint
$certThumbprint = $cert.Thumbprint

# Bind certificate to port
$guid = [guid]::NewGuid().ToString("B")
netsh http add sslcert ipport=0.0.0.0:5031 certhash=$certThumbprint appid="$guid"
```

**For Production - Use Let's Encrypt or Commercial SSL:**
- Install **Win-ACME** for free Let's Encrypt certificates
- Or purchase SSL from certificate authority

### B. Restrict CORS (Security Critical!)

Edit `Program.cs` before publishing:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
```

### C. Enable Request Filtering

```powershell
# Limit request size (prevent DoS)
Set-WebConfigurationProperty -PSPath "IIS:\Sites\AdminServer" `
    -Filter "system.webServer/security/requestFiltering/requestLimits" `
    -Name "maxAllowedContentLength" -Value 30000000

# Limit query string length
Set-WebConfigurationProperty -PSPath "IIS:\Sites\AdminServer" `
    -Filter "system.webServer/security/requestFiltering/requestLimits" `
    -Name "maxQueryString" -Value 2048
```

### D. Disable Directory Browsing

```powershell
Set-WebConfigurationProperty -PSPath "IIS:\Sites\AdminServer" `
    -Filter "system.webServer/directoryBrowse" `
    -Name "enabled" -Value $false
```

### E. Configure Application Pool Recycling

```powershell
# Recycle daily at 3 AM
Set-ItemProperty IIS:\AppPools\AdminServerPool -Name "recycling.periodicRestart.time" -Value "00:00:00"
Set-ItemProperty IIS:\AppPools\AdminServerPool -Name "recycling.periodicRestart.schedule" -Value @{value="03:00:00"}
```

---

## Step 8: Test Your Deployment

1. **Start the website:**
   ```powershell
   Start-Website -Name "AdminServer"
   ```

2. **Check application pool:**
   ```powershell
   Start-WebAppPool -Name "AdminServerPool"
   ```

3. **Test locally:**
   - Browse to: http://localhost:5030
   - Swagger UI: http://localhost:5030/swagger

4. **Check from network:**
   - Find your IP: `ipconfig`
   - Test from another device: http://YOUR_IP:5030

---

## Step 9: Monitoring & Logging

### Enable Detailed Errors (Development Only)

In `web.config`, set:
```xml
<aspNetCore processPath="dotnet"
            arguments=".\AdminServerStub.dll"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout"
            hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Development" />
  </environmentVariables>
</aspNetCore>
```

### View Logs

**IIS Logs:** `C:\inetpub\logs\LogFiles\W3SVC*`

**Application Logs:** `C:\inetpub\AdminServer\logs\`

**Event Viewer:**
- Windows Logs → Application
- Filter by Source: "IIS AspNetCore Module"

### Monitor Performance

```powershell
# Check if site is running
Get-Website -Name "AdminServer"

# Check app pool status
Get-WebAppPoolState -Name "AdminServerPool"

# View worker processes
Get-IISAppPool | Select-Object Name, State, @{n="WorkerProcesses";e={$_.WorkerProcesses.Count}}
```

---

## Step 10: Troubleshooting

### Common Issues

**1. 500.19 Error - Configuration Error**
- Check `web.config` syntax
- Verify ASP.NET Core Module is installed
- Restart: `iisreset`

**2. 502.5 Error - Process Failure**
- Check .NET 8 hosting bundle is installed
- Verify application pool identity has permissions
- Check stdout logs: `C:\inetpub\AdminServer\logs\`

**3. 403 Error - Forbidden**
- Check folder permissions
- Verify IIS_IUSRS and App Pool identity have access

**4. Application won't start**
```powershell
# Check detailed error
Set-ItemProperty IIS:\Sites\AdminServer -Name "applicationDefaults.preloadEnabled" -Value $true

# Run application manually to see errors
cd C:\inetpub\AdminServer
dotnet AdminServerStub.dll
```

**5. SignalR WebSocket issues**
- Ensure WebSockets are enabled in IIS features
- Check firewall allows WebSocket connections
- Verify `web.config` has `<webSocket enabled="true" />`

### Restart Commands

```powershell
# Restart IIS completely
iisreset /restart

# Restart specific site
Restart-WebAppPool -Name "AdminServerPool"
Stop-Website -Name "AdminServer"
Start-Website -Name "AdminServer"

# Clear browser cache and test
```

---

## Security Checklist

- [ ] Install latest Windows updates
- [ ] Configure firewall rules (only necessary ports)
- [ ] Use HTTPS with valid SSL certificate
- [ ] Restrict CORS to specific origins
- [ ] Disable directory browsing
- [ ] Enable request filtering and size limits
- [ ] Use strong authentication (implement JWT/OAuth)
- [ ] Regular security audits
- [ ] Monitor logs for suspicious activity
- [ ] Keep .NET runtime updated
- [ ] Use application pool identity (not Network Service)
- [ ] Remove development error pages in production
- [ ] Implement rate limiting
- [ ] Regular backups
- [ ] Use Windows Defender or antivirus

---

## Production Recommendations

### 1. Add Authentication

Implement JWT authentication in your `Program.cs`:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* configure */ });
```

### 2. Add Database

Replace in-memory storage with SQL Server or PostgreSQL

### 3. Configure Reverse Proxy (Optional)

For production, consider using IIS as reverse proxy to Kestrel:
- Better performance
- More control over the Kestrel process

### 4. Set Up Monitoring

- Application Insights
- ELMAH
- Serilog with SQL sink

### 5. Backup Strategy

```powershell
# Backup IIS configuration
Backup-WebConfiguration -Name "AdminServer-Backup-$(Get-Date -Format 'yyyyMMdd')"

# Backup application files
Copy-Item -Path "C:\inetpub\AdminServer" -Destination "D:\Backups\AdminServer-$(Get-Date -Format 'yyyyMMdd')" -Recurse
```

---

## Quick Reference Commands

```powershell
# Start/Stop Site
Start-Website -Name "AdminServer"
Stop-Website -Name "AdminServer"

# Start/Stop App Pool
Start-WebAppPool -Name "AdminServerPool"
Stop-WebAppPool -Name "AdminServerPool"

# Restart IIS
iisreset

# Check site status
Get-Website -Name "AdminServer" | Select-Object Name, State, PhysicalPath

# View bindings
Get-WebBinding -Name "AdminServer"

# Test configuration
Test-Path "C:\inetpub\AdminServer\AdminServerStub.dll"
Test-Path "C:\inetpub\AdminServer\web.config"
```

---

## Next Steps After Deployment

1. ✅ Test all API endpoints via Swagger
2. ✅ Test SignalR hubs (admin and agent)
3. ✅ Verify remote access from agents
4. ✅ Monitor logs for first 24 hours
5. ✅ Set up SSL certificate
6. ✅ Configure domain name (if applicable)
7. ✅ Implement authentication
8. ✅ Set up automated backups
9. ✅ Configure monitoring/alerts
10. ✅ Document server details for your team

---

**Your server will be accessible at:**
- Local: http://localhost:5030
- Network: http://YOUR_IP_ADDRESS:5030
- Swagger: http://YOUR_IP_ADDRESS:5030/swagger

**Need help?** Check logs in:
- `C:\inetpub\AdminServer\logs\`
- Event Viewer → Windows Logs → Application
