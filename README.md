# Admin Server

A .NET 8 ASP.NET Core API server for managing remote agents with real-time monitoring and command execution capabilities.

## Features

- **Agent Management**: Register and track multiple agents with unique identities
- **Real-time Metrics**: Monitor CPU, memory, disk usage, and network statistics
- **Command Execution**: Send commands to agents and receive results
- **Enhanced Data Collection**: System info, Windows info, disk info, antivirus info
- **Network Port Monitoring**: Track network connections and ports
- **Software Inventory**: Monitor installed software across agents
- **SignalR Hubs**: Real-time communication via WebSockets
- **Swagger UI**: Interactive API documentation

## Technology Stack

- .NET 8.0
- ASP.NET Core Web API
- SignalR for real-time communication
- In-memory data storage
- Swagger/OpenAPI documentation

## Project Structure

```
AdminServer/
├── AdminServerStub/
│   ├── Controllers/          # API Controllers
│   │   ├── AdminController.cs
│   │   ├── AgentController.cs
│   │   ├── CommandController.cs
│   │   ├── EnhancedDataController.cs
│   │   ├── InstalledSoftwareController.cs
│   │   └── NetworkPortController.cs
│   ├── Infrastructure/       # Core infrastructure
│   │   └── InMemoryStore.cs
│   ├── Models/              # Data models
│   │   └── Dtos.cs
│   ├── Program.cs           # Application entry point
│   └── AdminServerStub.csproj
├── Dockerfile               # Docker configuration
├── .dockerignore           # Docker ignore file
├── railway.json            # Railway configuration
└── README.md
```

## API Endpoints

### Agent Management
- `POST /api/Agent/register` - Register a new agent
- `POST /api/Agent/metrics` - Submit system metrics
- `POST /api/Agent/heartbeat/{agentId}` - Send heartbeat

### Admin Dashboard
- `GET /api/Admin/agents` - Get all agents
- `GET /api/Admin/agents/{agentId}` - Get specific agent
- `GET /api/Admin/agents/{agentId}/metrics` - Get agent metrics

### Command Execution
- `POST /api/Command` - Send command to agent
- `GET /api/Command/pending/{agentId}` - Get pending commands
- `GET /api/Command/{commandId}` - Get command result
- `POST /api/Command/result` - Submit or list command results

### Enhanced Data
- `POST /api/EnhancedData/submit` - Submit enhanced data
- `GET /api/EnhancedData/{agentId}/system-info` - Get system info
- `GET /api/EnhancedData/{agentId}/windows-info` - Get Windows info
- `GET /api/EnhancedData/{agentId}/harddisk-info` - Get disk info
- `GET /api/EnhancedData/{agentId}/antivirus-info` - Get antivirus info

### Network & Software
- `POST /api/NetworkPort` - Submit network port data
- `GET /api/NetworkPort/{agentId}` - Get network ports
- `POST /api/InstalledSoftware` - Submit software inventory
- `GET /api/InstalledSoftware/{agentId}/latest` - Get latest software list

### SignalR Hubs
- `/adminHub` - Admin hub for real-time updates
- `/agentHub` - Agent hub for agent connections

## Local Development

### Prerequisites
- .NET 8.0 SDK

### Running Locally

1. Navigate to the project directory:
   ```bash
   cd AdminServer/AdminServerStub
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. Access the API:
   - API: http://localhost:5030
   - Swagger UI: http://localhost:5030/swagger

## Docker Deployment

### Build Docker Image

```bash
docker build -t adminserver .
```

### Run Docker Container

```bash
docker run -p 5030:5030 adminserver
```

## Railway Deployment

This project is configured for easy deployment on Railway.

### Prerequisites
- Railway account (https://railway.app)
- Git repository (GitHub, GitLab, or Bitbucket)

### Deployment Steps

1. **Push to Git Repository**
   ```bash
   git init
   git add .
   git commit -m "Initial commit"
   git remote add origin <your-repo-url>
   git push -u origin main
   ```

2. **Deploy on Railway**
   - Go to https://railway.app
   - Click "New Project"
   - Select "Deploy from GitHub repo"
   - Choose your repository
   - Railway will automatically detect the Dockerfile and deploy

3. **Configure Environment (Optional)**
   - Railway automatically sets the `PORT` environment variable
   - No additional configuration needed

4. **Access Your Deployment**
   - Railway will provide a public URL (e.g., `https://your-app.railway.app`)
   - Access Swagger UI at: `https://your-app.railway.app/swagger`
   - API endpoints at: `https://your-app.railway.app/api/*`

### Railway Configuration

The project includes a `railway.json` file with the following settings:
- **Builder**: Dockerfile
- **Restart Policy**: ON_FAILURE
- **Max Retries**: 10

### Environment Variables

The application automatically reads the `PORT` environment variable provided by Railway:
```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "5030";
```

## Configuration

### CORS
The application is configured with permissive CORS (AllowAnyOrigin) for development. For production, consider restricting origins:

```csharp
options.AddDefaultPolicy(policy =>
    policy.WithOrigins("https://yourdomain.com")
          .AllowAnyHeader()
          .AllowAnyMethod());
```

### Port Configuration
The application listens on:
- Local development: Port 5030
- Railway: Port from `PORT` environment variable

## Security Considerations

⚠️ **Important**: This is a development/demo server with the following considerations:

1. **In-Memory Storage**: All data is stored in memory and will be lost on restart
2. **No Authentication**: API endpoints are not protected
3. **Permissive CORS**: Allows requests from any origin
4. **System Management**: Uses Windows Management Instrumentation (WMI) on Windows servers

For production use, consider:
- Implementing authentication/authorization (JWT, OAuth)
- Adding persistent storage (database)
- Restricting CORS to specific domains
- Adding rate limiting
- Implementing logging and monitoring
- Using HTTPS/TLS

## Troubleshooting

### Port Already in Use
If port 5030 is already in use locally:
```bash
# Change the default port in Program.cs
var port = Environment.GetEnvironmentVariable("PORT") ?? "5040";
```

### Railway Deployment Failed
1. Check build logs in Railway dashboard
2. Verify Dockerfile is in the root directory
3. Ensure all dependencies are in the .csproj file

### SignalR Connection Issues
- Ensure WebSocket support is enabled
- Check CORS configuration
- Verify the hub paths match client expectations

## Support

For issues and questions:
- Check the Swagger UI documentation at `/swagger`
- Review Railway deployment logs
- Check .NET 8 compatibility

## License

[Your License Here]

## Contributors

[Your Name/Team]
