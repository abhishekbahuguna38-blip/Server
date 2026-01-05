# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore
COPY AdminServer.sln ./
COPY AdminServerStub/AdminServerStub.csproj AdminServerStub/
COPY AdminServerStub.Tests/AdminServerStub.Tests.csproj AdminServerStub.Tests/
RUN dotnet restore AdminServerStub/AdminServerStub.csproj

# Copy everything and build
COPY . ./
RUN dotnet publish AdminServerStub/AdminServerStub.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway provides PORT env var; Program.cs already uses it and binds 0.0.0.0
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Default port (Railway will set PORT at runtime)
EXPOSE 5030

ENTRYPOINT ["dotnet", "AdminServerStub.dll"]
