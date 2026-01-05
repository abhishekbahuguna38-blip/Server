using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS Configuration - Controls which websites/apps can access your API
builder.Services.AddCors(options =>
{
    // Development: Allow everything (for testing)
    options.AddPolicy("DevelopmentPolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
    
    // Production: Allow specific origins + Cloudflare Tunnel
    options.AddPolicy("ProductionPolicy", policy =>
    {
        var allowedOrigins = new List<string>
        {
            // Local development URLs
            "http://localhost:3000",
            "http://localhost:5000",
            "http://localhost:5173",
            
            // Your production websites (replace with actual URLs)
            "https://youradmindashboard.com",
            "https://client1dashboard.com",
            "https://client2dashboard.com"
        };
        
        // Add Cloudflare Tunnel URL from environment variable
        var cloudflareUrl = Environment.GetEnvironmentVariable("CLOUDFLARE_TUNNEL_URL");
        if (!string.IsNullOrEmpty(cloudflareUrl))
        {
            allowedOrigins.Add(cloudflareUrl);
        }
        
        policy.WithOrigins(allowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();                  // Allow cookies/auth headers
    });
});

var app = builder.Build();

// Use appropriate CORS policy based on environment
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevelopmentPolicy");  // Allow all for testing
}
else
{
    app.UseCors("ProductionPolicy");   // Restrict to specific origins
}

// Enable Swagger in all environments for API testing
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHub<AdminHub>("/adminHub");
// Also map the agent hub path expected by some agents
app.MapHub<AdminHub>("/agentHub");

// Bind to the configured port for both development and production hosting scenarios
app.Run();


// Minimal hub for Admin real-time updates
public class AdminHub : Hub {}
