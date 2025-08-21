## Multi-stage Dockerfile for ASP.NET Core (.NET 10)
## Ensures ENTRYPOINT is an executable to avoid flag-only execution errors

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY VulnerableWebApp/VulnerableWebApp.csproj VulnerableWebApp/
RUN dotnet restore VulnerableWebApp/VulnerableWebApp.csproj

# Copy the rest of the source
COPY . .
WORKDIR /src/VulnerableWebApp

# Publish (self-contained trimming is optional; keeping framework-dependent for faster image)
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false /p:RunAnalyzersDuringBuild=false /p:RunAnalyzersDuringCompilation=false /p:NuGetAudit=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Non-root user is recommended; ASP.NET images provide dotnet user by default in some tags.
# If not present, comment these out or adjust UID/GID as needed.
# USER $APP_UID

COPY --from=build /app/publish .

# Expose the port your app listens on (match ASP.NET defaults or your configured port)
EXPOSE 8080

# Configure ASP.NET Core to listen on port 8080 by default
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Entrypoint must be the executable, not flags
ENTRYPOINT ["dotnet", "VulnerableWebApp.dll"]

# Use CMD for optional runtime args (example shown; commented by default)
# CMD ["--some-arg", "value"]

