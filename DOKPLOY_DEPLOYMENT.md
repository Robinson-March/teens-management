# Dokploy VPS Deployment Guide: Teens Church Management

This document provides step-by-step instructions to deploy both the frontend and backend of **Teens Church Management** to your VPS using **Dokploy**.

---

## Architecture Overview

There are two deployment approaches supported out of the box:

| Approach | Architecture | Best For |
|---|---|---|
| **Option A (Recommended)** | **All-in-One Compose Stack** (`docker-compose.yml`)<br>• `app` (.NET 10 API + embedded static frontend on port 8080)<br>• `db` (PostgreSQL 16 with persistent volume) | Fastest setup, zero CORS issues, lowest resource consumption on VPS |
| **Option B (Isolated Containers)** | **Multi-Container Stack** (`docker-compose.separate.yml`)<br>• `frontend` (Nginx on port 80)<br>• `backend` (.NET 10 on port 8080)<br>• `db` (PostgreSQL 16) | If you require a standalone Nginx edge proxy |

---

## Option A: Deploy via Dokploy Compose (Recommended)

### Step 1: Open Dokploy Dashboard
1. Log in to your Dokploy control panel (`http://<your-vps-ip>:3000`).
2. Go to **Projects** and select or create a project (e.g. `Teens-Church`).

### Step 2: Create a Compose Service
1. Click **Create Service** → select **Compose**.
2. Give it a name (e.g., `teens-management`).
3. Choose deployment source:
   - **Git Repository**: Enter your Git repository URL and branch (`main`).
   - **Docker Compose (Raw)**: Or paste the contents of `docker-compose.yml`.

### Step 3: Set Environment Variables
In the Dokploy **Environment** tab, set:
```bash
POSTGRES_DB=TeensChurchDb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_strong_secret_password_here
APP_PORT=8080
ASPNETCORE_ENVIRONMENT=Production
```

### Step 4: Configure Domain & SSL
1. Under the **Domains** section in Dokploy:
   - **Domain**: `teens.yourdomain.com` (or your chosen subdomain).
   - **Service / Container**: `app` (or port `8080`).
   - **Certificate**: Select **Let's Encrypt** (Dokploy will automatically generate and renew free SSL certificates via Traefik).

### Step 5: Deploy
1. Click **Deploy**.
2. Dokploy will pull images, build the .NET container, spin up PostgreSQL with persistent storage, run database migrations & seeding, and attach your domain with SSL.
3. Access your application at `https://teens.yourdomain.com`.

---

## Option B: Deploy as Individual Dokploy Services

If you prefer using Dokploy's native Managed Database UI + Application UI:

### 1. Create PostgreSQL in Dokploy
1. Under **Services**, click **Create Service** → **Database** → **PostgreSQL**.
2. Set Database Name: `TeensChurchDb`, User: `postgres`, and set a password.
3. Copy the internal connection string provided by Dokploy (e.g. `Host=teens-db;Port=5432;Database=TeensChurchDb;Username=postgres;Password=...`).

### 2. Create Application in Dokploy
1. Under **Services**, click **Create Service** → **Application**.
2. Connect your Git repository.
3. Set **Build Type**: `Dockerfile`.
4. Set **Dockerfile Path**: `Dockerfile` (or `backend/Dockerfile` with build context `/backend`).
5. Set **Port**: `8080`.
6. Add Environment Variable:
   ```bash
   ConnectionStrings__DefaultConnection=Host=<internal-db-host>;Port=5432;Database=TeensChurchDb;Username=postgres;Password=<your-db-password>
   ASPNETCORE_ENVIRONMENT=Production
   ```
7. Click **Deploy**.

---

## Verifying Deployment

Once deployed, you can verify:
- **Frontend Dashboard**: `https://your-domain.com/`
- **Swagger Documentation**: `https://your-domain.com/swagger`
- **Health Check**: `https://your-domain.com/api/health`

```bash
curl -f https://your-domain.com/api/health
# Response: {"status":"Healthy","timestamp":"...","database":"PostgreSQL"}
```
