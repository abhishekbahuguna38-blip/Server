# Use the official .NET 8 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["AdminServerStub/AdminServerStub.csproj", "AdminServerStub/"]
COPY ["AdminServerStub.Tests/AdminServerStub.Tests.csproj", "AdminServerStub.Tests/"]
RUN dotnet restore "AdminServerStub/AdminServerStub.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AdminServerStub"
RUN dotnet build "AdminServerStub.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AdminServerStub.csproj" -c Release -o /app/publish

# Build the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the port to match Render's expectation
ENV ASPNETCORE_URLS=http://+:8080
ENV PORT=8080

ENTRYPOINT ["dotnet", "AdminServerStub.dll"]
