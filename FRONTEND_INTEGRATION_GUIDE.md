# Frontend Integration Guide for Depix Next.js Application

## 1. Executive Summary
This document provides a comprehensive analysis of the Depix backend architecture, network configuration, security models, and API endpoints to facilitate the integration of the separate Next.js frontend repository.

The backend is a **.NET 8 Modular Monolith** (`CrmErp`) providing unified CRM, Sales, Customer, Project, Finance, Support, Platform, and Reporting capabilities. It runs inside Docker containers orchestrated via Nginx as an API Gateway.

This analysis explicitly distinguishes between **Confirmed Facts** (verified directly from backend code, configuration files, and Docker definitions), **Recommendations** (optimal integration patterns for Next.js), and **Unresolved Questions & Assumptions** (items requiring confirmation from the frontend team or infrastructure operators).

---

## 2. Confirmed Backend Ports and Endpoints

### Framework & Runtime
- **Runtime / SDK**: .NET 8.0 (`mcr.microsoft.com/dotnet/aspnet:8.0` and `sdk:8.0`)
- **Entry Point**: `CrmErp.Host.dll` (`src/Host/CrmErp.Host/Program.cs`)
- **Dockerfile Location**: `Dockerfile` in the root repository directory.

### Port Binding & Network Exposure
- **Internal Listening Port inside Container**: `http://0.0.0.0:5000` (configured via `ENV ASPNETCORE_URLS=http://+:5000` in `Dockerfile`).
- **Host Binding**: The backend container binds to `0.0.0.0` inside its network container namespace. Its port `5000` is **NOT** published directly to the host machine in `docker-compose.yml` or `docker-compose.prod.yml`. Access is restricted strictly through internal Docker networks and Nginx proxying.
- **HTTPS Termination**: Expected to take place at the Nginx API Gateway level (or reverse proxy). ASP.NET Core runs as plain HTTP on port `5000` behind Nginx, utilizing `UseForwardedHeaders()` configured in `src/Host/CrmErp.Host/Program.cs` for `X-Forwarded-For`, `X-Forwarded-Proto`, and `X-Forwarded-Host`.

### Health Check Endpoints
- **Container Health Check**: `http://localhost:5000/health` (used by Docker healthchecks in `docker-compose.yml` and `docker-compose.prod.yml`).
- **Unified Health API**: `GET /api/v1/health` (returns `{"status":"Healthy","system":"Depix API","timestamp":"..."}`).

### OpenAPI & Documentation Endpoints
- **Swagger UI**: `GET /swagger/index.html` (proxied in Nginx as `GET /api/docs` and `GET /swagger/`).
- **OpenAPI JSON Spec**: `GET /swagger/v1/swagger.json` (proxied in Nginx as `GET /api/openapi.json`).
- **Configuration Control**: OpenAPI endpoints are controlled by `OPENAPI_ENABLED` (boolean, defaults to `true`).

### Available API Module Endpoints
All API controllers expose routes under `/api/`:
- **Auth**: `/api/auth` (`login`, `refresh`, `logout`, `me`)
- **CRM**: `/api/crm/leads`, `/api/crm/companies`, `/api/crm/contacts`, `/api/crm/activities`, `/api/crm/sales-notes`
- **Sales**: `/api/sales/opportunities`, `/api/sales/proposals`
- **Customer**: `/api/customers`
- **Project**: `/api/projects`
- **Finance**: `/api/finance/invoices`
- **Support**: `/api/support`
- **Platform**: `/api/platform/tasks`, `/api/platform/files`, `/api/platform/audit-logs`, `/api/platform/settings`
- **Reporting**: `/api/reports`

---

## 3. Authentication and CORS Requirements

### Authentication Scheme
- **Mechanism**: JWT Bearer Tokens (`bearerAuth`).
- **Token Transmission**: Requests to protected endpoints must include the HTTP header:
  ```http
  Authorization: Bearer <access_token>
  ```
- **Login Endpoint**: `POST /api/auth/login`
  - Request body: `{ "username": "...", "password": "..." }` (or email/username credentials).
  - Response payload: Includes `accessToken`, `refreshToken`, `accessTokenExpiration`, `refreshTokenExpiration`, and `user` details wrapped in `ApiResponse<T>`.
- **Token Refresh**: `POST /api/auth/refresh`
  - Request body: `{ "accessToken": "...", "refreshToken": "..." }`.
- **Logout Endpoint**: `POST /api/auth/logout` (Requires `Authorization: Bearer <access_token>`).
- **Current User Endpoint**: `GET /api/auth/me` (Requires `Authorization: Bearer <access_token>`).

### Cookies & Token Storage Strategy
- **Confirmed Fact**: The backend API does not set HTTP-only cookies directly in `AuthController.cs`. Access and refresh tokens are returned in the JSON response body.
- **Frontend Recommendation**: Next.js client component calls or BFF (Backend-For-Frontend) route handlers should store refresh tokens securely (e.g. in `HttpOnly`, `SameSite=Lax`, `Secure` cookies managed by Next.js middleware / API routes) to mitigate XSS exposure, while passing the Bearer token in headers to the backend.

### CORS Policy Configuration
- **Policy Name**: `DefaultCorsPolicy` (`src/Host/CrmErp.Host/Program.cs`).
- **Configuration Key**: `Cors:AllowedOrigins` (array or environment variable `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`).
- **Behavior**:
  - Configured origins are allowed with `.AllowAnyHeader()`, `.AllowAnyMethod()`, and `.AllowCredentials()`.
  - If no origins are configured, `policy.SetIsOriginAllowed(_ => false)` rejects cross-origin browser requests.
  - In `docker-compose.prod.yml`, `Cors__AllowedOrigins__0=${CORS_ALLOWED_ORIGIN}` sets the permitted origin.

### File Upload Constraints
- **Endpoint**: `POST /api/platform/files` (multipart/form-data).
- **Required Form Fields**: `file` (IFormFile), `entityType` (string, max 100 chars), `entityId` (Guid). Optional: `description` (string).
- **Validation Limits** (`src/Modules/Platform/Application/Validators/PlatformValidators.cs`):
  - Maximum file size: **100 MB** (`100 * 1024 * 1024` bytes).
  - File size must be greater than 0 bytes.

### Response & Error Handling Contract
- **Wrapper Structure** (`ApiResponse<T>` in `BuildingBlocks.Common.Responses`):
  ```json
  {
    "success": true,
    "message": "Operation completed successfully",
    "data": { ... },
    "errors": [],
    "traceId": "0HN012345678"
  }
  ```
- **Error Structure**: When `success` is `false`, `errors` contains objects with `code`, `field` (optional), and `message`.
- **Standard HTTP Error Mapping**:
  - `400 Bad Request`: Validation failure (`VALIDATION_ERROR`) or Business Rule failure (`BUSINESS_RULE_VIOLATION`).
  - `401 Unauthorized`: Missing or invalid JWT.
  - `403 Forbidden`: Authenticated user lacks required permission claim (e.g. `Sales.Proposal.Create`).
  - `404 Not Found`: Entity not found (`NOT_FOUND`).
  - `500 Internal Server Error`: Unhandled server exception (`SYSTEM_001`).

---

## 4. Docker Network and Service-Name Requirements

### Network Architecture
- **Development Network** (`docker-compose.yml`): `depix-network` (bridge).
- **Production Network** (`docker-compose.prod.yml`): `crmerp-prod-network` (bridge).
- **External Gateway Network Context**: Context notes state the deployment server Nginx gateway uses Docker network `depix_depix-network`.

### Service Names and Aliases
- **Backend Service Name**: `backend` (container name `crmerp-backend` in dev, `crmerp-prod-backend` in prod).
- **Frontend Service Name**: `frontend` (container name `depix-frontend` in dev, `crmerp-prod-frontend` in prod).
- **Nginx Gateway Service Name**: `nginx` (container name `depix-gateway` in dev, `crmerp-prod-nginx` in prod).

### Internal Communication
- **Backend Internal URL**: `http://backend:5000`
- **Frontend Internal URL**: `http://frontend:3000`
- **Port Exposure**: The backend port `5000` is exposed **internally** within the Docker network. It must **NOT** be published publicly on the host (e.g. do not map `5000:5000` on host).

---

## 5. Nginx and Production Routing Recommendations

### Existing Nginx Configuration Overview (`infrastructure/nginx/conf.d/default.conf`)
The production Nginx configuration currently defines:
- **Upstream Definitions**:
  ```nginx
  upstream backend_server {
      server backend:5000;
  }
  upstream frontend_server {
      server frontend:3000;
  }
  ```
- **Current Locations**:
  - `location /`: Proxies to `http://frontend_server` with WebSocket upgrade headers (`Upgrade`, `Connection`).
  - `location /_next/static/`: Proxies to `http://frontend_server/_next/static/` with 365-day cache header.
  - `location /back-end/`: Proxies to `http://backend_server/` (strips `/back-end/` prefix).
  - `location /health`: Proxies to `http://backend_server/health`.
  - `location /swagger/`: Proxies to `http://backend_server/swagger/`.
  - `location /api/docs`: Proxies to `http://backend_server/swagger/`.
  - `location /api/openapi.json`: Proxies to `http://backend_server/swagger/v1/swagger.json`.
  - `location /api/v1/health`: Proxies to `http://backend_server/api/v1/health`.

### CRITICAL ROUTING CONFLICT & RECOMMENDATION
1. **The Issue**:
   - Backend controllers serve endpoints under `/api/...` (e.g., `/api/auth/login`, `/api/customers`, `/api/projects`).
   - Nginx currently routes `/back-end/` to `http://backend_server/` (which strips `/back-end/`), but browser requests calling `/api/auth/login` directly on Nginx will fall through to `location /` (the Next.js frontend) and return a 404 or Next.js page.
2. **Recommendation for Production Gateway Alignment**:
   - The Nginx configuration should explicitly proxy `/api/` to `http://backend_server/api/` without stripping the `/api/` prefix.
   - Example Nginx location block needed:
     ```nginx
     location /api/ {
         proxy_pass http://backend_server/api/;
         proxy_http_version 1.1;
         proxy_set_header Host $host;
         proxy_set_header X-Real-IP $remote_addr;
         proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
         proxy_set_header X-Forwarded-Proto $scheme;
         proxy_set_header X-Forwarded-Host $host;
     }
     ```
3. **Frontend Immediate Workaround**:
   - If Nginx cannot be modified immediately, frontend browser requests can use `/back-end/api/...` as the public base URL, which Nginx maps to `http://backend_server/api/...`.

---

## 6. Frontend Environment Variable Names and Example Placeholders

Below are the environment variables recommended for the Next.js frontend application repository:

```env
# Public API Base URL for Client-side Browser Requests
# Option A (Standard /api proxy):
NEXT_PUBLIC_API_BASE_URL=https://depix.ir/api

# Option B (Using existing /back-end proxy path if /api is not routed):
# NEXT_PUBLIC_API_BASE_URL=https://depix.ir/back-end/api

# Internal API Base URL for Server-Side Rendering (SSR) & Next.js Server Actions
# Communicates directly over the internal Docker network to backend service
INTERNAL_API_BASE_URL=http://backend:5000/api

# Optional Next.js Application Environment Variables
NEXT_TELEMETRY_DISABLED=1
NODE_ENV=production
```

*Note: Never place private keys, database passwords, or JWT secrets in Next.js public environment variables (`NEXT_PUBLIC_*`).*

---

## 7. GitHub Actions and Deployment Recommendations

### Existing CI Pipeline Analysis (`.github/workflows/ci.yml`)
- The backend repository currently runs a single CI workflow (`build-and-test`) on push/PR to `main`/`master`:
  1. Setup .NET 8 SDK
  2. `dotnet restore CrmErp.sln`
  3. `dotnet build CrmErp.sln --no-restore --configuration Release`
  4. `dotnet test CrmErp.sln --no-build --configuration Release`

### Existing Deployment Script Analysis (`scripts/deploy.sh`)
- Uses `docker-compose.prod.yml`.
- Runs database migrations via temporary SDK container or `dotnet-ef`.
- Verifies system health via `curl -s http://localhost/health`.

### Recommended Frontend CI/CD Strategy (Separate Repository)
Since the Next.js frontend resides in a separate Git repository:
1. **Independent CI Workflow**:
   - Next.js repository should have its own `.github/workflows/ci.yml` running `npm test`, `npm run lint`, and `docker build`.
2. **Container Registry Tagging**:
   - Build frontend Docker image using multi-stage Dockerfile (`output: 'standalone'`).
   - Tag images with Git commit SHA and `latest` (e.g. `registry.depix.ir/depix-frontend:${{ github.sha }}`).
3. **Deployment Integration**:
   - The production server uses `docker-compose.prod.yml` where `frontend` is defined.
   - For automated deployment, GitHub Actions on the frontend repository can trigger a webhook or SSH deployment step that executes:
     ```bash
     docker pull registry.depix.ir/depix-frontend:latest
     docker compose -f docker-compose.prod.yml up -d frontend
     ```
4. **GitHub Actions Secrets (By Name Only)**:
   - `DEPLOY_HOST` (Production server IP/domain)
   - `DEPLOY_USER` (SSH user)
   - `SSH_PRIVATE_KEY` (SSH key for deployment trigger)
   - `REGISTRY_USERNAME` / `REGISTRY_PASSWORD` (Container registry credentials)

---

## 8. Unresolved Questions and Assumptions

### Confirmed Facts Summary
- [x] Backend framework is **.NET 8.0** listening internally on `0.0.0.0:5000`.
- [x] Authentication is **JWT Bearer** token-based via `/api/auth/*`.
- [x] File upload limit is strictly **100 MB**.
- [x] CORS allowed origins are dynamically read from `Cors:AllowedOrigins` (`Cors__AllowedOrigins__0`).
- [x] All API responses follow the `ApiResponse<T>` wrapper contract.

### Key Recommendations
- [x] Standardize Next.js public API calls to use `/api` base URL once Nginx routes `/api/` directly to `http://backend:5000/api/`.
- [x] Next.js SSR / Server-side requests should use `http://backend:5000/api` directly over the Docker internal network (`depix-network` / `crmerp-prod-network`).
- [x] Next.js should manage refresh token storage via secure, HttpOnly, SameSite cookies on Next.js route handlers if browser local storage is deemed vulnerable to XSS.

### Unresolved Questions & Operational Assumptions
1. **Nginx Routing Update**: Will the operations team update `infrastructure/nginx/conf.d/default.conf` to route `location /api/` to `http://backend_server/api/`, or should the frontend temporarily target `/back-end/api/`?
2. **Docker Network Harmonization**: In production, is the shared Docker network named `depix_depix-network` or `crmerp-prod-network`? The frontend container definition in `docker-compose.prod.yml` must join the exact same external bridge network as Nginx and backend.
3. **SSR vs CSR Routing**: Does the Next.js team plan to perform client-side fetching (CSR) only, or will they leverage Server-Side Rendering (SSR) / Server Actions? If SSR is used, `INTERNAL_API_BASE_URL=http://backend:5000` must be reachable from the Next.js container.
