# Depix API — Unified Swagger & OpenAPI Specification

## 1. Overview & Repository Audit

This repository (`depix`) houses the backend infrastructure for the **Depix Platform**.

The core underlying backend service is implemented as a .NET 8 Modular Monolith CRM/ERP system (`CrmErp`). This specification establishes the OpenAPI/Swagger Documentation Layer representing the Depix backend architecture.

---

## 2. Target Architecture & API Gateway

```text
                    ┌──────────────────────┐
                    │        Nginx         │
                    │    API Gateway       │
                    └──────────┬───────────┘
                               │
                       Unified API Surface
                               │
                    ┌──────────▼──────────┐
                    │    .NET CRM/ERP     │
                    │  Modular Monolith   │
                    └─────────────────────┘
```

The Nginx Reverse Proxy / API Gateway routes incoming requests from the single unified external surface (`http://localhost:8080` or production domain) to internal services:

- `/api/docs` -> Unified Swagger UI Entry Point
- `/api/openapi.json` -> Raw OpenAPI Specification
- `/api/v1/health` -> Unified Health Check
- `/api/auth/*` & `/api/sales/*`, `/api/crm/*`, `/api/customer/*`, `/api/project/*`, `/api/finance/*`, `/api/support/*`, `/api/platform/*` -> Core .NET Modular Monolith API

---

## 3. Swagger UI & Raw OpenAPI Endpoints

| Resource | Path | Description |
| :--- | :--- | :--- |
| **Swagger UI** | `/api/docs` | Interactive Swagger UI documentation |
| **OpenAPI Spec (JSON)** | `/api/openapi.json` | Raw OpenAPI Specification (JSON) |
| **Health Endpoint** | `/api/v1/health` | Service & Database Health Status |

---

## 4. OpenAPI Specification Details

```yaml
openapi: 3.1.0
info:
  title: Depix API
  description: Unified API for Depix CRM/ERP Operations
  version: 1.0.0
servers:
  - url: /
    description: Unified API Gateway (Current Environment)
  - url: http://localhost:8080
    description: Local Docker / Gateway Development Server
```

---

## 5. Security & Authentication Schemes

The unified API standardizes on JWT Bearer Token authentication for user endpoints:

```yaml
components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
      description: Enter the JWT bearer token obtained from POST /api/auth/login.
```

### Endpoint Security Requirements

| Endpoint Category | Security Requirement | Description |
| :--- | :--- | :--- |
| **System & Health** (`/health`, `/api/v1/health`) | `security: []` | Public |
| **Auth APIs** (`/api/auth/login`, `/api/auth/refresh`) | `security: []` | Public |
| **Admin APIs** (`/api/sales/*`, `/api/crm/*`, `/api/customer/*`, `/api/project/*`, `/api/finance/*`, `/api/support/*`, `/api/platform/*`) | `security: [bearerAuth]` | Admin/Staff JWT + Permission Claim |

---

## 6. Common Response Contracts & Error Models

All API endpoints follow a unified response structure (`ApiResponse<T>`):

### Success Response (`200 OK` / `201 Created`)
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Acme Website Development"
  },
  "errors": [],
  "traceId": "0HN012345678"
}
```

### Validation Error Response (`400 Bad Request`)
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "field": "email",
      "message": "Email is required."
    }
  ],
  "traceId": "0HN012345678"
}
```

---

## 7. API Inventory & Tag Categorization

| Category Tag | Description | Implementation Status |
| :--- | :--- | :--- |
| **System** | `/health`, `/api/v1/health`, `/api/docs`, `/api/openapi.json` | `IMPLEMENTED` |
| **Authentication** | `/api/auth/login`, `/api/auth/refresh`, `/api/auth/logout`, `/api/auth/me` | `IMPLEMENTED` |
| **CRM** | Leads, Companies, Contacts, Activities, Sales Notes | `IMPLEMENTED` |
| **Sales** | Opportunities, Opportunity Pipeline, Proposals | `IMPLEMENTED` |
| **Customer** | Customer Profiles, Accounts | `IMPLEMENTED` |
| **Project** | Projects, Requirements, Tech Stacks, Repositories, Deployments | `IMPLEMENTED` |
| **Finance** | Contracts, Invoices, Payment Schedules, Transactions | `IMPLEMENTED` |
| **Support** | Support Plans, SLA Rules, Support Tickets | `IMPLEMENTED` |
| **Platform** | File Storage, Operational Tasks, Audit Logging | `IMPLEMENTED` |
| **Reporting** | Business metrics & pipeline analytics | `IMPLEMENTED` |

---

## 8. Environment Configuration

The OpenAPI and Swagger UI behaviors are controlled by environment configuration:

```env
OPENAPI_ENABLED=true
```

---

## 9. Local Development & Docker Usage

### Running via Docker Compose
```bash
docker compose up --build
```
Access points:
- Swagger UI: `http://localhost:8080/api/docs`
- OpenAPI Specification: `http://localhost:8080/api/openapi.json`
- Gateway Health: `http://localhost:8080/api/v1/health`

### Verification & Testing
Run solution tests:
```bash
dotnet test
```
