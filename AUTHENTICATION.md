# Depix CRM/ERP Monolith — Authentication & Authorization Architecture

## 1. Authentication Architecture Overview

The Depix Platform uses an ASP.NET Core Modular Monolith (.NET 8) for CRM/ERP operations behind an API Gateway managed by Nginx.

```text
                         Depix API Gateway (Nginx)
                                     │
                               Admin / Staff Realm
                                     │
                             Authorization Policy
                                     │
                                .NET CRM/ERP
                                (PostgreSQL)
```

---

## 2. Authentication Domains & Mechanisms

### Admin & Staff Authentication
- **Primary Authority**: .NET Identity Module.
- **Scope**: CRM leads, sales opportunities, proposals, project management, financial invoicing, SLA support tickets, system settings.
- **Mechanisms**: JWT Bearer Tokens (`bearerAuth`) issued by .NET Identity Module.

---

## 3. Permission Definitions

Granular permission strings follow the pattern `{Module}.{Entity}.{Action}`:

- `Customer.Create`, `Customer.Read`, `Customer.Update`, `Customer.Delete`, `Customer.Assign`, `Customer.StatusChange`
- `Sales.Opportunity.Create`, `Sales.Opportunity.Read`, `Sales.Proposal.Create`, `Sales.Proposal.Update`
- `Project.Create`, `Project.Read`, `Project.Update`, `Project.Deployment.Create`
- `Finance.Contract.Create`, `Finance.Invoice.Create`, `Finance.Transaction.Read`
- `Support.Ticket.Create`, `Support.Ticket.Read`, `Support.Ticket.Resolve`
- `Platform.File.Upload`, `Platform.AuditLog.Read`

---

## 4. API Access Control Matrix

| API Category | Path / Endpoint | Public | Admin / Staff | Authentication Scheme |
| :--- | :--- | :---: | :---: | :--- |
| **Health Check** | `/health`, `/api/v1/health` | ✓ | ✓ | None |
| **OpenAPI Docs** | `/api/docs`, `/api/openapi.json` | ✓ | ✓ | None |
| **Auth API** | `/api/auth/*` | ✓ | ✓ | Public for Login/Refresh |
| **Core CRM / ERP Monolith** | `/api/crm/*`, `/api/sales/*`, `/api/customer/*`, etc. | ✕ | ✓ | JWT Bearer (`bearerAuth`) |

---

## 5. Security Policies & Practices

### A. Passwords & Sensitive Data
- Passwords must be hashed using PBKDF2 via ASP.NET Core Identity.
- Passwords and JWT secrets are **never** logged, returned in responses, or embedded in OpenAPI sample responses.

### B. Session & Token Security
- JWTs expire in short windows (e.g., 60 minutes) with secure refresh token rotation.

### C. Rate Limiting & CORS
- Rate limiting is active on API Host.
- CORS origins configured via `Cors__AllowedOrigins` environment variables.

---

## 6. Environment Variables Reference

```env
# JWT Configuration
Jwt__Secret=YOUR_SECURE_JWT_SECRET
Jwt__Issuer=DepixPlatform
Jwt__Audience=DepixClients

# CORS Configuration
Cors__AllowedOrigins__0=http://localhost:8080

# Gateway & OpenAPI
OPENAPI_ENABLED=true
```
