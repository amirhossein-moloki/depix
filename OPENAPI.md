# Depix API — Unified Swagger & OpenAPI Specification

## 1. Overview & Repository Audit

This repository (`depix`) houses the backend infrastructure for the **Depix Platform**.

While the core underlying backend service is implemented as a .NET 8 Modular Monolith CRM/ERP system (`CrmErp`), this specification establishes a **Unified OpenAPI/Swagger Documentation Layer** representing the complete Depix backend architecture, including the integration boundary between Commerce (Medusa), CMS (Payload), and CRM/ERP operational microservices.

---

## 2. Target Architecture & API Gateway

```text
                    ┌──────────────────────┐
                    │        Nginx         │
                    │    API Gateway       │
                    └──────────┬───────────┘
                               │
                       Unified API Surface
            ┌──────────────────┼──────────────────┐
            │                  │                  │
    /api/v1/commerce/*    /api/v1/cms/*   /api/v1/integration/*
            │                  │                  │
     ┌──────▼──────┐    ┌─────▼─────┐    ┌────────▼────────┐
     │   Medusa    │    │  Payload  │    │ .NET CRM/ERP    │
     │  Commerce   │    │    CMS    │    │ Modular Monolith│
     └──────┬──────┘    └─────┬─────┘    └────────┬────────┘
            │                 │                   │
            └───────── Integration Boundary ──────┘
```

The Nginx Reverse Proxy / API Gateway routes incoming requests from the single unified external surface (`http://localhost:8080` or production domain) to internal services:

- `/api/docs` -> Unified Swagger UI Entry Point
- `/api/openapi.json` -> Raw OpenAPI 3.1.0 / 3.0.0 Specification
- `/api/v1/commerce/*` -> Medusa Commerce Engine
- `/api/v1/cms/*` -> Payload CMS Engine
- `/api/v1/integration/*` -> Commerce ↔ CMS Integration Boundary
- `/api/v1/health` -> Unified Health Check
- `/api/auth/*` & `/api/sales/*`, `/api/crm/*`, `/api/customer/*`, `/api/project/*`, `/api/finance/*` -> Core .NET Modular Monolith API

---

## 3. Swagger UI & Raw OpenAPI Endpoints

| Resource | Path | Description |
| :--- | :--- | :--- |
| **Swagger UI** | `/api/docs` | Interactive Swagger UI documentation |
| **OpenAPI Spec (JSON)** | `/api/openapi.json` | Raw OpenAPI Specification (JSON) |
| **OpenAPI Spec (YAML)** | `/api/openapi.yaml` | Raw OpenAPI Specification (YAML) |
| **Health Endpoint** | `/api/v1/health` | Service & Database Health Status |

---

## 4. OpenAPI Specification Details

```yaml
openapi: 3.1.0
info:
  title: Depix API
  description: Unified API for Depix Commerce, CMS, and CRM/ERP Operations
  version: 1.0.0
servers:
  - url: /
    description: Unified API Gateway (Current Environment)
  - url: http://localhost:8080
    description: Local Docker / Gateway Development Server
```

---

## 5. Security & Authentication Schemes

The unified API standardizes on JWT Bearer Token authentication for user endpoints and Internal API Keys for service-to-service integration:

```yaml
components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
      description: Enter the JWT bearer token obtained from POST /api/auth/login.
    internalApiKey:
      type: apiKey
      in: header
      name: X-Internal-API-Key
      description: Internal Service-to-Service API Key for integration endpoints.
```

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

### Unified Error Structure Schema
```yaml
ApiError:
  type: object
  properties:
    code:
      type: string
      example: VALIDATION_ERROR
    field:
      type: string
      nullable: true
      example: email
    message:
      type: string
      example: Email is invalid
```

---

## 7. Medusa ↔ Payload Integration Boundary

### Relationship & Data Ownership
- **Medusa Engine**: Primary owner of transaction, inventory, pricing, cart, and order domain objects (`product.id`).
- **Payload CMS**: Primary owner of content, articles, pages, rich text, media, and marketing copy.
- **Identifier Boundary**: Payload content documents link directly to Medusa using `medusa_product_id`.

```text
  Medusa Product (product.id) <──── [medusa_product_id] ──── Payload CMS Page / Content
```

### Integration Contract Endpoints

#### `GET /api/v1/integration/products/{productId}`
- **Status**: `IMPLEMENTED`
- **Summary**: Retrieves merged product details combining Medusa commerce pricing/stock with Payload rich content.

#### `GET /api/v1/integration/products/{productId}/content`
- **Status**: `IMPLEMENTED`
- **Summary**: Fetches CMS content associated with a given Medusa product ID (`medusa_product_id`).

#### `POST /api/v1/integration/products/{productId}/sync`
- **Status**: `IMPLEMENTED`
- **Summary**: Triggers catalog synchronization between Medusa commerce events and Payload CMS content caches.
- **Security**: Requires `X-Internal-API-Key` header.

#### `POST /api/v1/integration/webhooks`
- **Status**: `IMPLEMENTED`
- **Summary**: Receives webhook events from Medusa and Payload to execute asynchronous catalog synchronization.
- **Security**: Requires `X-Internal-API-Key` header.

---

## 8. API Inventory & Tag Categorization

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
| **Commerce** | `/api/v1/commerce/products`, `/cart`, `/orders`, `/payments`, `/inventory` | `PLANNED` |
| **CMS** | `/api/v1/cms/pages`, `/articles`, `/authors`, `/media`, `/seo` | `PLANNED` |
| **Integration** | `/api/v1/integration/products/{id}`, `/sync`, `/webhooks` | `IMPLEMENTED` |

---

## 9. Environment Configuration

The OpenAPI and Swagger UI behaviors are controlled by environment configuration:

```env
OPENAPI_ENABLED=true
OPENAPI_BASE_URL=http://localhost:8080
SWAGGER_ENABLED=true
```

Ensure no credentials or private keys are hardcoded in environment templates (`.env.example`).

---

## 10. Local Development & Docker Usage

### Running via Docker Compose
```bash
docker-compose up --build
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
