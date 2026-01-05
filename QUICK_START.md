# ⚡ AdminServer - Quick Start Guide

## 🎯 What is This?

AdminServer is a .NET 8 API for managing remote agents with real-time monitoring, command execution, and system data collection.

## 🚀 Quick Deploy to Railway (5 Minutes)

### Option 1: Railway CLI (Fastest)

```bash
# Install Railway CLI
npm i -g @railway/cli

# Login
railway login

# Deploy
cd c:\Users\ASUS\Desktop\AdminServer
railway init
railway up
```

Done! Your app is live at the provided URL.

### Option 2: GitHub + Railway Web (Recommended)

```bash
# 1. Push to GitHub
cd c:\Users\ASUS\Desktop\AdminServer
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/YOUR_USERNAME/AdminServer.git
git push -u origin main

# 2. Deploy on Railway
# - Go to https://railway.app
# - Click "New Project" → "Deploy from GitHub repo"
# - Select your repository
# - Wait for deployment (2-5 minutes)
# - Click "Generate Domain" in Settings
```

Done! Access your app at: `https://your-app.railway.app/swagger`

## 🖥️ Test Locally First

### Run with .NET (Development)

```powershell
.\run-local.ps1
```

Access at: http://localhost:5030/swagger

### Test with Docker (Production-like)

```powershell
.\test-docker.ps1
```

This tests the same Docker image Railway will use.

## 📚 Key Files

| File | Purpose |
|------|---------|
| `Dockerfile` | Docker container configuration for Railway |
| `railway.json` | Railway platform settings |
| `.dockerignore` | Excludes unnecessary files from build |
| `.gitignore` | Git ignore patterns |
| `RAILWAY_DEPLOYMENT.md` | Detailed deployment guide |
| `DEPLOYMENT_CHECKLIST.md` | Step-by-step checklist |

## 🌐 API Endpoints (After Deployment)

Replace `YOUR_URL` with your Railway URL:

### Test Connection
```bash
curl https://YOUR_URL/api/admin/agents
```

### Register Agent
```bash
curl -X POST https://YOUR_URL/api/agent/register \
  -H "Content-Type: application/json" \
  -d '{
    "machineName": "MyPC",
    "ipAddress": "192.168.1.100",
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "operatingSystem": "Windows 11"
  }'
```

### View Swagger UI
```
https://YOUR_URL/swagger
```

## 🔧 Environment Variables

Railway automatically sets:
- `PORT` - Application port (handled automatically)

Optional variables you can add:
- `ASPNETCORE_ENVIRONMENT` - Set to `Production` (default)

## 📊 What Gets Deployed?

- ✅ .NET 8 ASP.NET Core API
- ✅ SignalR real-time hubs
- ✅ Swagger UI for API testing
- ✅ In-memory data storage
- ✅ CORS enabled (all origins)
- ✅ Multiple controllers (Agent, Command, Admin, etc.)

## 🎯 Project Structure

```
AdminServer/
├── AdminServerStub/          # Main application
│   ├── Controllers/          # API endpoints
│   ├── Infrastructure/       # Data storage
│   ├── Models/              # Data models
│   └── Program.cs           # App configuration
├── Dockerfile               # Container config
├── railway.json            # Railway config
└── *.md                    # Documentation
```

## 🐛 Common Issues

### Build Fails
```bash
# Check .NET version
dotnet --version  # Should be 8.x

# Test build locally
dotnet build AdminServerStub/AdminServerStub.csproj
```

### Docker Issues
```powershell
# Test Docker build
.\test-docker.ps1

# Check Docker
docker --version
```

### Railway Deployment Fails
1. Check Railway logs in dashboard
2. Verify Dockerfile is in root directory
3. Ensure all files are committed to Git

## 💡 Tips

1. **Test Locally First**: Run `.\run-local.ps1` before deploying
2. **Test Docker**: Run `.\test-docker.ps1` to verify container works
3. **Check Logs**: Always check Railway logs after deployment
4. **Use Swagger**: Test all endpoints via Swagger UI
5. **Monitor Usage**: Keep an eye on Railway's free tier limits

## 📖 More Information

- **Full Documentation**: See `README.md`
- **Deployment Guide**: See `RAILWAY_DEPLOYMENT.md`
- **Checklist**: See `DEPLOYMENT_CHECKLIST.md`

## 🆘 Need Help?

1. Check Railway logs: Railway Dashboard → Logs
2. Review local build: `dotnet build AdminServerStub/AdminServerStub.csproj`
3. Test Docker: `.\test-docker.ps1`
4. Railway Discord: https://discord.gg/railway
5. Railway Docs: https://docs.railway.app

## ✅ Success Indicators

Your deployment is working when:
- ✅ Railway shows "Success" status
- ✅ Domain is accessible
- ✅ Swagger UI loads at `/swagger`
- ✅ `/api/admin/agents` returns JSON (empty array or agent list)
- ✅ No errors in Railway logs

## 🎉 You're Done!

Once deployed:
1. Share your Railway URL with your team
2. Configure agents to connect to your URL
3. Monitor via Railway dashboard
4. Use Swagger UI to test APIs

**Example URL**: `https://adminserver-production.railway.app`

---

**Quick Commands Reference**

```bash
# Local development
.\run-local.ps1

# Test Docker
.\test-docker.ps1

# Deploy via CLI
railway up

# View logs
railway logs

# Open in browser
railway open
```

---

**Need detailed instructions?** → Open `RAILWAY_DEPLOYMENT.md`
**Ready to deploy?** → Follow `DEPLOYMENT_CHECKLIST.md`
