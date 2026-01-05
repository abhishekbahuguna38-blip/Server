# IIS Setup Guide for AdminServer

## Changes Made

### 1. ✅ IIS Support Restored
- Server now runs under IIS in production mode
- Runs as Kestrel in development mode (for testing with Cloudflare)

### 2. ✅ Port Information Endpoints Added
Your Admin dashboard can now fetch port info for all agents:

**New Endpoints:**
- `GET /api/Admin/ports` - Get all agent ports (grouped by agentId)
- `GET /api/Admin/ports/summary` - Get port summary with agent details
- `GET /api/Admin/agents/{agentId}/ports` - Get ports for specific agent
- `GET /api/Admin/agents/{agentId}/ports/latest` - Get latest ports for specific agent

## How to Deploy to IIS

### Step 1: Install IIS
```powershell
# Run PowerShell as Administrator
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HealthAndDiagnostics
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Security
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Performance
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerManagementTools
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ManagementConsole
```

### Step 2: Install ASP.NET Core Hosting Bundle
1. Download from: https://dotnet.microsoft.com/download/dotnet/8.0
2. Look for "Hosting Bundle" under Runtime section
3. Install and restart your PC

### Step 3: Publish Your Application
```powershell
cd c:\Users\ASUS\Desktop\AdminServer\AdminServerStub
dotnet publish -c Release -o c:\inetpub\AdminServer
```

### Step 4: Create IIS Application
```powershell
# Run PowerShell as Administrator
Import-Module WebAdministration

# Create Application Pool
New-WebAppPool -Name "AdminServerPool"
Set-ItemProperty IIS:\AppPools\AdminServerPool -Name "managedRuntimeVersion" -Value ""

# Create Website
New-Website -Name "AdminServer" `
    -Port 5030 `
    -PhysicalPath "c:\inetpub\AdminServer" `
    -ApplicationPool "AdminServerPool"

# Start the website
Start-Website -Name "AdminServer"
```

### Step 5: Configure Firewall
```powershell
New-NetFirewallRule -DisplayName "AdminServer IIS" -Direction Inbound -LocalPort 5030 -Protocol TCP -Action Allow
```

## Testing IIS Deployment

### 1. Check if IIS is running:
Visit: http://localhost:5030/swagger

### 2. Test port endpoints:
```powershell
# Get all agent ports
curl http://localhost:5030/api/Admin/ports

# Get port summary with agent details
curl http://localhost:5030/api/Admin/ports/summary

# Get ports for specific agent
curl http://localhost:5030/api/Admin/agents/{agentId}/ports
```

## Running in Development Mode (Kestrel)

For testing with Cloudflare Tunnel, run in development mode:

```powershell
cd c:\Users\ASUS\Desktop\AdminServer\AdminServerStub
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

This will:
- Use Kestrel (not IIS)
- Bind to 0.0.0.0:5030 (accessible externally)
- Allow all CORS origins
- Work with Cloudflare Tunnel

## Switching Between IIS and Kestrel

### Use IIS (Production):
- Deploy to IIS using steps above
- Server runs as Windows Service
- Managed through IIS Manager
- Set environment to "Production"

### Use Kestrel (Development):
```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

## Troubleshooting

### IIS Not Starting:
1. Check Application Pool is running
2. Verify .NET 8 Hosting Bundle is installed
3. Check Windows Event Viewer for errors

### Can't Access from External Network:
1. Verify Windows Firewall allows port 5030
2. Check IIS bindings (should be 0.0.0.0:5030 or *:5030)
3. Ensure router port forwarding is configured

### Port Data Not Showing:
1. Verify agents are sending data to `/api/NetworkPort`
2. Check Swagger UI for new endpoints
3. Test endpoints directly with curl/Postman

## Auto-Start Configuration

### IIS (Automatic):
IIS websites start automatically with Windows - no extra configuration needed.

### Kestrel (Manual):
Use the startup script we created earlier:
- Located at: `c:\Users\ASUS\Desktop\AdminServer\start-server.ps1`
- Already configured to auto-start on boot
