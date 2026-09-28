# ==========================================
# Stage 1: Build & Publish (.NET 10 SDK)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore NuGet packages
COPY TeensChurch.API.csproj ./
RUN dotnet restore TeensChurch.API.csproj

# Copy all source files and publish release
COPY . ./
RUN dotnet publish TeensChurch.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Stage 2: Production Runtime (.NET 10 ASP.NET Core)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for reliable container health monitoring
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Configure ASP.NET Core production environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Copy compiled artifacts including wwwroot static frontend
COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f -I http://localhost:8080/api/health || exit 1

ENTRYPOINT ["dotnet", "TeensChurch.API.dll"]
