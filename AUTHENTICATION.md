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
| **OpenAPI Docs** | `/api/docs`, `/api/openapi.json` | ✓ | ✓ | None |
| **Auth Endpoints** | `/api/auth/login`, `/api/auth/refresh`, `/api/auth/register` | ✓ | ✓ | None |
| **User Approval & Management** | `/api/identity/users/pending`, `/api/identity/users/{id}/approve`, `/api/identity/users/{id}/reject` | ✕ | ✓ | JWT Bearer (`bearerAuth`) |
| **Sales Visits & Geo-Location** | `/api/crm/visits`, `/api/crm/companies/{id}/visits` | ✕ | ✓ | JWT Bearer (`bearerAuth`) |
| **Core CRM / ERP Monolith** | `/api/auth/me`, `/api/sales/*`, `/api/customer/*`, `/api/project/*`, `/api/finance/*`, `/api/support/*`, `/api/platform/*` | ✕ | ✓ | JWT Bearer (`bearerAuth`) |

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
