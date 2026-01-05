# Quick Start - Deploy to IIS in 5 Minutes

## The Fastest Way to Deploy

### 1️⃣ Install .NET 8 Hosting Bundle (One-time)

**Download:**
- Go to: https://dotnet.microsoft.com/download/dotnet/8.0
- Download: **ASP.NET Core Runtime 8.0.x - Windows Hosting Bundle**
- Install and **RESTART YOUR COMPUTER**

### 2️⃣ Run Automated Setup

**Open PowerShell as Administrator** (Right-click PowerShell → Run as Administrator)

```powershell
cd C:\Users\ASUS\Desktop\AdminServer
.\setup-iis.ps1
```

**That's it!** 🎉

Your server will be running at:
- http://localhost:5030
- http://localhost:5030/swagger

---

## Optional: Add HTTPS

After running `setup-iis.ps1`, run:

```powershell
.\setup-ssl.ps1
```

Your server will now support HTTPS:
- https://localhost:5031

---

## Troubleshooting

### Script won't run?
```powershell
# Enable script execution (run once)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Port already in use?
```powershell
# Use a different port
.\setup-iis.ps1 -Port 8080
```

### .NET 8 not found?
1. Download hosting bundle from link above
2. Install it
3. **RESTART your computer** (critical!)
4. Verify: `dotnet --list-runtimes`

### Site not starting?
```powershell
# Check logs
Get-Content C:\inetpub\AdminServer\logs\stdout_*.log -Tail 50

# Restart IIS
iisreset
```

---

## Manual Deployment (If Script Fails)

If the automated script doesn't work, follow the detailed guide:
- See `IIS_DEPLOYMENT_GUIDE.md` for complete step-by-step instructions

---

## Access from Other Devices

### 1. Find your IP address:
```powershell
ipconfig
```
Look for "IPv4 Address" (e.g., 192.168.1.100)

### 2. Access from network:
- http://YOUR_IP:5030
- Example: http://192.168.1.100:5030

### 3. If connection fails:
- Check Windows Firewall (should be configured by script)
- Or manually add firewall rule in Windows Defender Firewall

---

## Useful Commands

```powershell
# Check site status
Get-Website -Name "AdminServer"

# Restart site
Restart-WebAppPool -Name "AdminServerPool"

# View recent logs
Get-Content C:\inetpub\AdminServer\logs\stdout_*.log -Tail 50

# Stop site
Stop-Website -Name "AdminServer"

# Start site
Start-Website -Name "AdminServer"

# Complete IIS restart
iisreset
```

---

## Security Checklist (Before Going Live)

- [ ] Install .NET 8 Hosting Bundle
- [ ] Run setup-iis.ps1
- [ ] Test site locally
- [ ] Set up HTTPS (run setup-ssl.ps1)
- [ ] Update CORS in Program.cs (restrict origins)
- [ ] Implement authentication (JWT recommended)
- [ ] Change default ports if needed
- [ ] Test from other devices
- [ ] Monitor logs for 24 hours
- [ ] Set up backups

---

## Production Deployment

For production, you should:

1. **Get a real SSL certificate**
   - Use Let's Encrypt (free) with Win-ACME
   - Or buy from a Certificate Authority

2. **Secure your API**
   - Add authentication (JWT/OAuth)
   - Restrict CORS to your domains
   - Implement rate limiting

3. **Use a database**
   - Replace in-memory storage
   - SQL Server, PostgreSQL, etc.

4. **Set up monitoring**
   - Application Insights
   - Log aggregation
   - Alerts for errors

5. **Configure backups**
   - Regular IIS config backups
   - Database backups
   - Application files backup

---

## Need Help?

1. Check the detailed guide: `IIS_DEPLOYMENT_GUIDE.md`
2. View logs: `C:\inetpub\AdminServer\logs\`
3. Check Event Viewer: Windows Logs → Application
4. Test manually: `cd C:\inetpub\AdminServer` then `dotnet AdminServerStub.dll`

---

**Questions? Check the logs first - they usually tell you what's wrong!**
