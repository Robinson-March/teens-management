# ==========================================
# Root Dockerfile: Full-Stack Container (.NET 10 + Frontend)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY backend/TeensChurch.API.csproj ./backend/
RUN dotnet restore ./backend/TeensChurch.API.csproj

# Copy all source files
COPY backend/ ./backend/
COPY code.html ./backend/wwwroot/index.html

# Publish
WORKDIR /src/backend
RUN dotnet publish TeensChurch.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/api/health || exit 1

ENTRYPOINT ["dotnet", "TeensChurch.API.dll"]
