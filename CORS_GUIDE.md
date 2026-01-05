# CORS Configuration Guide

## What is CORS?

**CORS (Cross-Origin Resource Sharing)** is a security feature that controls which websites/applications can access your API.

### Without CORS:
- Any website on the internet could call your API
- Hackers could steal data or abuse your server
- **Very dangerous!**

### With CORS:
- Only approved websites can access your API
- Blocks unauthorized access
- **Much more secure!**

---

## How It Works in Your App

### Development Mode (Testing):
```csharp
app.UseCors("DevelopmentPolicy");  // Allows ALL origins
```
- Used when testing locally
- Allows Swagger, Postman, any tool to access API
- **NOT secure - only for development!**

### Production Mode (Real Use):
```csharp
app.UseCors("ProductionPolicy");   // Only allows specific origins
```
- Used when deployed to Railway/IIS
- Only allows websites you specify
- **Secure - blocks unauthorized access**

---

## How to Configure for Your Needs

### Step 1: Update Allowed Origins

In `Program.cs`, find this section:
```csharp
options.AddPolicy("ProductionPolicy", policy =>
    policy.WithOrigins(
        "http://localhost:3000",           // ← Add your admin dashboard URL
        "https://youradmin.com",           // ← Add your production domain
        "https://yourdomain.com"           // ← Add any other trusted sites
    )
```

### Step 2: Add Your URLs

Replace with your actual URLs:
```csharp
policy.WithOrigins(
    "http://localhost:5000",              // Your local admin app
    "https://myadmindashboard.com",       // Your production admin
    "https://abc123.ngrok-free.app"       // Your ngrok URL (if using)
)
```

### Step 3: Redeploy

After changing CORS settings:
```powershell
.\redeploy.ps1
```

---

## Common Scenarios

### Scenario 1: Testing Locally
**Use Development Mode:**
- Allows all origins
- Easy for testing
- Not secure

### Scenario 2: Production with Admin Dashboard
**Use Production Mode:**
```csharp
policy.WithOrigins(
    "https://youradmindashboard.com"
)
```

### Scenario 3: Multiple Admin Sites
**Add all trusted domains:**
```csharp
policy.WithOrigins(
    "https://admin1.com",
    "https://admin2.com",
    "https://dashboard.com"
)
```

### Scenario 4: Mobile Apps + Web
**Mobile apps don't need CORS**, but web does:
```csharp
policy.WithOrigins(
    "https://yourwebapp.com"  // Only web app needs CORS
)
// Mobile apps work without CORS
```

---

## Testing CORS

### Test 1: From Browser Console
Open your admin dashboard, press F12, run:
```javascript
fetch('http://localhost:5030/api/Admin/agents')
  .then(r => r.json())
  .then(data => console.log(data))
  .catch(err => console.error('CORS blocked!', err));
```

### Test 2: Check Response Headers
Look for these headers in browser dev tools:
```
Access-Control-Allow-Origin: https://yourdomain.com
Access-Control-Allow-Methods: GET, POST, PUT, DELETE
```

---

## Common CORS Errors

### Error: "CORS policy: No 'Access-Control-Allow-Origin' header"
**Solution:** Add your website URL to `WithOrigins()`

### Error: "CORS policy: The value of the 'Access-Control-Allow-Origin' header must not be the wildcard '*'"
**Solution:** You're using `.AllowCredentials()` with `.AllowAnyOrigin()` - not allowed! Use specific origins.

### Error: "CORS policy: Credentials flag is true, but Access-Control-Allow-Credentials is false"
**Solution:** Add `.AllowCredentials()` to your policy

---

## Security Best Practices

### ✅ DO:
- Use specific origins in production
- Use HTTPS URLs in production
- Test CORS before deploying
- Update origins when URLs change

### ❌ DON'T:
- Use `.AllowAnyOrigin()` in production
- Allow `http://` in production (use `https://`)
- Forget to update CORS when adding new admin sites
- Mix `.AllowAnyOrigin()` with `.AllowCredentials()`

---

## Quick Reference

| Environment | Policy | Security | Use Case |
|-------------|--------|----------|----------|
| Development | `DevelopmentPolicy` | ⚠️ Low | Local testing |
| Production | `ProductionPolicy` | ✅ High | Real deployment |

---

## Need Help?

1. **Can't access API from browser?** → Check CORS policy includes your URL
2. **Works in Postman but not browser?** → CORS issue, add your domain
3. **Mobile app can't connect?** → Mobile apps don't use CORS, check other issues

---

**Remember:** CORS only affects browser-based requests. Desktop apps, mobile apps, and server-to-server calls don't need CORS!
