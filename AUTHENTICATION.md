# Depix Platform — Authentication & Authorization Architecture

## 1. Authentication Architecture Overview

The Depix Platform uses **.NET 8 Modular Monolith** (CRM/ERP Operations) served under a unified API Gateway managed by Nginx.

```text
                         Depix API Gateway (Nginx)
                                     │
                        Authorization & Routing
                                     │
                            .NET CRM/ERP Host
                              (PostgreSQL)
```

---

## 2. Authentication Domain

Depix uses JWT Bearer authentication for secure user and administrative sessions.

### Admin & User Authentication
- **Primary Authority**: .NET Identity Module.
- **Scope**: CRM leads, proposals, project management, customer management, financial invoicing, SLA support tickets, platform settings.
- **Mechanisms**: JWT Bearer Tokens (`bearerAuth`) issued by .NET Identity (`/api/auth/login`, `/api/auth/refresh`).
- **Security Rule**: Protected routes evaluate permission-based policy claims.

---

## 3. Roles & Permissions Mapping

| Unified Depix Role | .NET CRM/ERP Permission Claims |
| :--- | :--- |
| **Super Admin** | `*:*:*` (Full System Superuser) |
| **Sales Manager** | `Sales.Proposal.Create`, `Customer.Read`, `Customer.Update` |
| **Support Lead** | `Support.Ticket.Create`, `Support.Ticket.Read`, `Support.Ticket.Resolve` |
| **Platform Admin** | `Platform.File.Create`, `Platform.File.Read`, `Platform.AuditLog.Read` |

---

## 4. Permission Definitions

Granular permission strings follow the pattern `{Module}.{Entity}.{Action}`:

- `Customer.Create`, `Customer.Read`, `Customer.Update`, `Customer.Delete`, `Customer.Assign`, `Customer.StatusChange`
- `Sales.Opportunity.Create`, `Sales.Opportunity.Read`, `Sales.Proposal.Create`, `Sales.Proposal.Update`
- `Project.Create`, `Project.Read`, `Project.Update`, `Project.Deployment.Create`
- `Finance.Contract.Create`, `Finance.Invoice.Create`, `Finance.Transaction.Read`
- `Support.Ticket.Create`, `Support.Ticket.Read`, `Support.Ticket.Resolve`
- `Platform.File.Upload`, `Platform.AuditLog.Read`

---

## 5. API Access Control Matrix

| API Category | Path / Endpoint | Public | Admin / Staff | Authentication Scheme |
| :--- | :--- | :---: | :---: | :--- |
| **Health Check** | `/health`, `/api/v1/health` | ✓ | ✓ | None |
| **OpenAPI Docs** | `/api/docs`, `/api/redoc`, `/api/openapi.json` | ✓ | ✓ | None |
| **Auth Endpoints** | `/api/auth/login`, `/api/auth/register`, `/api/auth/refresh` | ✓ | ✓ | None |
| **User Approval & Admin Endpoints** | `/api/users`, `/api/users/pending`, `/api/users/{id}/approve`, `/api/users/{id}/reject` | ✕ | Admin | JWT Bearer (`bearerAuth`) |
| **Sales Visits Endpoints** | `/api/crm/sales-visits`, `/api/crm/companies/{id}/visits` | ✕ | Staff | JWT Bearer (`bearerAuth`) |
| **Core CRM / ERP Monolith** | `/api/auth/me`, `/api/sales/*`, `/api/customer/*`, `/api/project/*`, `/api/finance/*`, `/api/support/*`, `/api/platform/*` | ✕ | ✓ | JWT Bearer (`bearerAuth`) |

---

## 5.1 CLI Administrator Creation Tool

Administrators can be created directly and securely via local executable CLI command without relying on public endpoints:

```bash
dotnet run --project src/Host/CrmErp.Host/CrmErp.Host.csproj -- create-admin --email admin@depix.ir --password 'SecurePassword123!'
```

If flags are omitted, interactive CLI prompts securely collect inputs with masked password entry. Admin accounts created via CLI are assigned active status immediately.

---

## 5.2 CLI Password Reset Tool

User passwords can be reset directly and securely via local executable CLI command:

```bash
dotnet run --project src/Host/CrmErp.Host/CrmErp.Host.csproj -- reset-password --email user@depix.ir --password 'NewSecurePassword123!'
```

Both `--password` and `--new-password` argument flags are supported. If flags are omitted, interactive CLI prompts collect missing inputs with masked password entry. Resetting a password revokes all active refresh tokens for the user.

---

## 5.3 Registration & User Approval Lifecycle

1. **Public Registration**: Users register via `POST /api/auth/register`. Registrations default to `PendingApproval` status and do not issue JWT access tokens.
2. **Admin User Creation**: Managers create active users directly via `POST /api/users`.
3. **Approval Lifecycle**:
   - `GET /api/users/pending`: Retrieves pending registrations.
   - `POST /api/users/{id}/approve`: Approves account for login access.
   - `POST /api/users/{id}/reject`: Rejects account request and revokes active refresh tokens.

---

## 6. Security Policies & Practices

### A. Passwords & Sensitive Data
- Passwords must be hashed using ASP.NET Core Identity PBKDF2.
- Passwords and JWT secrets are **never** logged, returned in responses, or embedded in OpenAPI sample responses.

### B. Session & Token Security
- JWTs expire in short windows (e.g., 60 minutes) with secure refresh token rotation.

### C. Rate Limiting & CORS
- Rate limiting is active on API Host (100 requests/min).
- CORS origins configured via `Cors:AllowedOrigins` environment variable (no `*` wildcard on authenticated routes).

---

## 7. Environment Variables Reference

```env
# JWT Configuration
Jwt__Secret=YOUR_SECURE_JWT_SECRET
Jwt__Issuer=CrmErpApi
Jwt__Audience=CrmErpClients

# CORS Configuration
Cors__AllowedOrigins__0=http://localhost:3000
Cors__AllowedOrigins__1=http://localhost:8080

# Gateway & OpenAPI
OPENAPI_ENABLED=true
OPENAPI_BASE_URL=http://localhost:8080
```

---

## 8. Verification Results

All security specifications and authentication mechanisms are verified by automated test suites:
- **.NET Monolith Test Suite**: verified passing (`dotnet test`).
