# Depix E-commerce — Unified Authentication & Authorization Architecture

## 1. Authentication Architecture Overview

The Depix Platform combines **Medusa Engine** (Commerce), **Payload Engine** (CMS), **.NET 8 Modular Monolith** (CRM/ERP Operations), and the **Node.js Integration Layer** under a unified API Gateway managed by Nginx.

```text
                         Depix API Gateway (Nginx)
                                     │
        ┌────────────────────────────┼────────────────────────────┐
        │                            │                            │
   Public Surface              Customer Realm                 Admin Realm
  (Catalog/Content)          (Medusa Customer)            (Staff/Managers)
        │                            │                            │
        └────────────────────────────┼────────────────────────────┘
                                     │
                             Authorization Policy
                                     │
        ┌────────────────────────────┴────────────────────────────┐
        │                            │                            │
   Medusa Engine               Payload CMS                 .NET CRM/ERP
  (Commerce DB)                (Payload DB)                (PostgreSQL)
        │                            │                            │
        └────────────── Internal Service Key (X-Internal) ────────┘
```

---

## 2. Authentication Domains

To preserve system isolation and security boundaries, Depix establishes 3 non-overlapping authentication domains:

```text
┌────────────────────────────────────────────────────────┐
│                      Depix Auth                        │
├────────────────────────────────────────────────────────┤
│ 1. Customer Auth   │ 2. Admin Auth   │ 3. Service Auth │
│ (Medusa Commerce)  │ (Framework/IAM) │ (X-Internal Key)│
└────────────────────────────────────────────────────────┘
```

### A. Customer Authentication
- **Primary Authority**: Medusa Commerce Engine.
- **Scope**: Shopping carts, customer checkout, order history, addresses, customer profile.
- **Mechanisms**: Medusa session cookies / JWT tokens.
- **Security Rule**: Customers must never be granted access to internal operational APIs or administrative endpoints.

### B. Admin Authentication
- **Primary Authority**: Framework-specific administrative authenticators (Medusa Admin, Payload CMS Access Control, .NET Identity Module).
- **Scope**: Catalog management, CMS content publishing, CRM leads, proposals, project management, financial invoicing, SLA support tickets.
- **Mechanisms**: JWT Bearer Tokens (`bearerAuth`) issued by .NET Identity or Medusa/Payload Admin authenticators.
- **Security Rule**: Admin accounts are separated from customer buyer identities.

### C. Service-to-Service Authentication
- **Primary Authority**: Integration Layer & Internal Gateway Security.
- **Scope**: Cross-system catalog synchronization (`Medusa ↔ Integration ↔ Payload`) and webhooks.
- **Mechanisms**: Header-based internal service key (`X-Internal-API-Key`).
- **Security Rule**: Public consumers and customer JWTs are strictly forbidden from calling internal service/sync routes.

---

## 3. Roles & Permissions Mapping

| Unified Depix Role | Medusa Role / Permission | Payload Role / Access Control | .NET CRM/ERP Permission Claims |
| :--- | :--- | :--- | :--- |
| **Customer** | Registered Customer | Public Read | N/A |
| **Super Admin** | Admin (All) | Admin (All) | `*:*:*` (Full System Superuser) |
| **Commerce Manager** | Store Manager / Catalog Write | Product Metadata Editor | `Sales.Proposal.Create`, `Customer.Read`, `Customer.Update` |
| **Content Manager** | Catalog Read | Content Editor (Pages, Articles, Media) | `Platform.File.Create`, `Platform.File.Read` |
| **Integration Service** | Internal Webhook Producer | Internal Sync Client | Internal API Key (`X-Internal-API-Key`) |

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

| API Category | Path / Endpoint | Public | Customer | Admin / Staff | Internal Service | Authentication Scheme |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| **Health Check** | `/health`, `/api/v1/health` | ✓ | ✓ | ✓ | ✓ | None |
| **OpenAPI Docs** | `/api/docs`, `/api/openapi.json` | ✓ | ✓ | ✓ | ✓ | None |
| **Merged Product Catalog** | `GET /api/v1/integration/products/:id` | ✓ | ✓ | ✓ | ✓ | None |
| **CMS Public Content** | `GET /api/v1/integration/products/:id/content` | ✓ | ✓ | ✓ | ✓ | None |
| **Customer Auth & Cart** | `/api/v1/commerce/store/*` | ✕ | ✓ | ✓ | ✓ | Medusa Session / JWT |
| **Own Customer Orders** | `GET /api/v1/commerce/store/orders/:id` | ✕ | ✓ (Owner) | ✓ | ✓ | Medusa Customer JWT |
| **Catalog / CMS Write** | `/api/v1/commerce/admin/*`, `/api/v1/cms/*` | ✕ | ✕ | ✓ | ✓ | Admin JWT / Payload Auth |
| **Core CRM / ERP Monolith** | `/api/auth/*`, `/api/sales/*`, `/api/customer/*` | ✕ | ✕ | ✓ | ✕ | JWT Bearer (`bearerAuth`) |
| **Catalog Sync / Webhooks** | `POST /api/v1/integration/products/:id/sync` | ✕ | ✕ | ✕ | ✓ | Key (`X-Internal-API-Key`) |

---

## 6. Security Policies & Practices

### A. Passwords & Sensitive Data
- Passwords must be hashed using bcrypt/Argon2 or ASP.NET Core Identity PBKDF2.
- Passwords, JWT secrets, and API keys are **never** logged, returned in responses, or embedded in OpenAPI sample responses.

### B. Session & Token Security
- JWTs expire in short windows (e.g., 15-60 minutes) with secure refresh token rotation.
- Web cookies enforce `HttpOnly`, `Secure`, and `SameSite=Lax/Strict`.

### C. Service Authentication (`X-Internal-API-Key`)
- Evaluated in `infrastructure/integration/middleware/auth.ts` on protected integration endpoints.
- Mismatched or missing headers immediately yield `401 Unauthorized`.

### D. Rate Limiting & CORS
- Rate limiting is active on API Host (100 requests/min).
- CORS origins configured via `CORS_ORIGINS` environment variable (no `*` wildcard on authenticated routes).

---

## 7. Environment Variables Reference

```env
# JWT Configuration
Jwt__Secret=YOUR_SECURE_JWT_SECRET
Jwt__Issuer=CrmErp.Host
Jwt__Audience=CrmErp.Clients

# Integration Security
INTEGRATION_INTERNAL_API_KEY=YOUR_SECURE_INTERNAL_SERVICE_KEY

# CORS Configuration
CORS_ORIGINS=http://localhost:3000,http://localhost:8080

# Gateway & OpenAPI
OPENAPI_ENABLED=true
OPENAPI_BASE_URL=http://localhost:8080
```

---

## 8. Verification Results

All security specifications and authentication mechanisms are verified by automated test suites:
- **.NET Monolith Test Suite**: 246 tests passing (`dotnet test`).
- **Integration Layer Security Suite**: 6 tests passing (`npm test` in `infrastructure/integration`).
