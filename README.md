# CRM / ERP Modular Monolith Backend Architecture

Production-ready ASP.NET Core Modular Monolith architecture designed for a Web Development Company managing the complete lifecycle from potential leads to customers, contracts, website projects, deployments, support, and financial reporting.

---

## 1. Architectural Analysis & Design Strategy

### A. Executive Architectural Principles
- **Clean Architecture**: Strict separation of concerns (Domain, Application, Infrastructure, API). Domain layer has 0 external dependencies.
- **Modular Monolith**: High cohesive domain modules separated into physical project boundaries while deployed as a single unified ASP.NET Core host application (`CrmErp.Host`).
- **Domain-Driven Design (DDD)**: Entities, Aggregate Roots, Value Objects, Domain Events, and Repository contracts encapsulate business rules.
- **CQRS (Command Query Responsibility Segregation)**: Read/Write separation primitives included in `BuildingBlocks.Application`.
- **Database Schema Isolation**: Single PostgreSQL database instance with distinct database schemas per module (`identity`, `crm`, `sales`, `customer`, `project`, `finance`, `support`, `platform`).

### B. Module Boundary Separation Rationale
1. **Identity Module**: Handles user management, roles, claims, permissions, authentication (JWT/OAuth), refresh token rotation, and permission-based authorization policies.
2. **CRM Module**: Focuses on **pre-sales lead nurture and acquisition** (Leads, Companies, Contacts, Lead Research, Sales Notes, Lead Activities).
3. **Sales Module**: Handles **sales pipeline conversion** (Opportunities, Proposals, Proposal Items, Stages, Deal Tracking). Hands off qualified leads to Customer and Finance modules.
4. **Customer Module**: Manages **converted paying customers** and customer profiles. Clear separation between pre-sale leads (CRM) and active customer relationships (Customer).
5. **Project Module**: Core operational module for delivery. Handles Website and Software Projects, Technical Requirements, Tech Stacks, Git Repositories, Deployments, and Technical Documentation.
6. **Finance Module**: Handles financial tracking, Contracts, Payment Schedules/Items, Invoices, Payments, and Transaction records.
7. **Support Module**: Manages post-delivery services, Support Plans, Service Level Agreements (SLAs), Support Tickets, Comments, and History.
8. **Platform Module**: Provides shared internal capabilities including File Management, Internal Operational Tasks, Audit Logs, and Settings.
9. **Reporting Module**: Aggregates business performance metrics across pre-sales, active sales, finance, projects, and customer support.

### C. Communication Between Modules
- **Rule**: Direct project-to-project references or entity sharing between modules is strictly forbidden.
- **Contract Interfaces**: Cross-module communication relies on contract interfaces in `BuildingBlocks.Application.Contracts` (e.g. `ICustomerService`, `IProjectService`).
- **In-Memory Event Bus & Domain Events**: Modules communicate via domain events / integration events (`IEventBus`) for transactional consistency and eventual consistency.
- **Foreign Keys**: Primary keys (e.g. `Guid CustomerId`) are stored as primitive scalar values across module boundaries rather than navigational Entity properties.

---

## 2. Solution Structure

```
src/
├── BuildingBlocks/
│   ├── Common/                  # Middleware (Correlation ID), Global Exception Handler, Response Factory, Observability
│   ├── Domain/                  # Base Entity, AggregateRoot, AuditableEntity, ISoftDelete, ValueObject (Money), IDomainEvent
│   ├── Application/             # CQRS interfaces (ICommand, IQuery), Contract Interfaces, IUnitOfWork
│   ├── Infrastructure/          # ApplicationDbContext, EF Core base mappings, Interceptors, UnitOfWork
│   └── SharedKernel/            # Result<T>, Error, PagedResult<T> primitives
│
├── Modules/
│   ├── Identity/                # User, Role, RefreshToken, Auth Controllers
│   ├── CRM/                     # Company, Contact, Lead, LeadResearch, Activity, SalesNote
│   ├── Sales/                   # Opportunity, Proposal, ProposalItem, Stages
│   ├── Customer/                # Customer profile & lifecycle
│   ├── Project/                 # Project, Requirement, TechStack, Repository, Deployment
│   ├── Finance/                 # Contract, Invoice, InvoiceItem, Payment, Transaction
│   ├── Support/                 # SupportPlan, Ticket, TicketComment, TicketHistory
│   ├── Platform/                # File, Internal Task, AuditLog, Setting
│   └── Reporting/               # Business analytics & pipeline aggregations
│
└── Host/
    └── CrmErp.Host/             # ASP.NET Core Web API Host & Composition Root

tests/
├── CrmErp.ArchitectureTests/    # NetArchTest Clean Architecture & Module Isolation verification
├── Modules.Identity.Tests/     # JWT Token & Password Hasher Unit Tests
├── UnitTests/                   # Domain & Command/Query Handler Unit Tests
└── IntegrationTests/            # EF Core In-Memory & End-to-End Workflow Integration Tests
```

---

## 3. Global Dependency Rules

```
       [ CrmErp.Host ] (Composition Root)
          /        \
         v          v
   [Module.API]  [Module.Infrastructure]
        \            /
         v          v
     [Module.Application]
             |
             v
      [Module.Domain]
```

1. **Domain Layer**: Zero dependencies. Only references `BuildingBlocks.Domain` and `BuildingBlocks.SharedKernel`.
2. **Application Layer**: Depends only on `Module.Domain`, `BuildingBlocks.Application`, and `BuildingBlocks.SharedKernel`.
3. **Infrastructure Layer**: Depends on `Module.Application`, `Module.Domain`, and `BuildingBlocks.Infrastructure`.
4. **API Layer**: Depends only on `Module.Application` and `BuildingBlocks.Common`. Does NOT depend on Infrastructure.
5. **Host Layer (`CrmErp.Host`)**: Acts as the Composition Root. References all `Module.API` and `Module.Infrastructure` projects to register DI services and run the web server.

---

## 4. Operational & Deployment Guidelines

### A. Environment Configuration & Variables
Production settings must be supplied via environment variables or configuration providers rather than hardcoded secrets.

Key Configuration Keys:
- `ConnectionStrings__Database`: PostgreSQL connection string (e.g. `Host=postgres;Port=5432;Database=crm_erp_db;Username=postgres;Password=your_secure_password`).
- `Jwt__Secret`: 256-bit (32+ byte) secret key used for signing JWT tokens.
- `Jwt__Issuer`: JWT Issuer claim (e.g. `CrmErpApi`).
- `Jwt__Audience`: JWT Audience claim (e.g. `CrmErpClients`).
- `Jwt__AccessTokenExpirationMinutes`: Access token lifespan in minutes (default: `60`).
- `Jwt__RefreshTokenExpirationDays`: Refresh token lifespan in days (default: `7`).
- `Cors__AllowedOrigins`: JSON array or env vars specifying allowed CORS origins (e.g. `Cors__AllowedOrigins__0=https://crm.yourdomain.com`).
- `OPENAPI_ENABLED`: Set to `true` to enable Swagger UI and `/api/docs` documentation endpoint.
- `INTEGRATION_INTERNAL_API_KEY`: API Key for service-to-service integration endpoints.

### B. Database Migrations
Migrations are maintained under `src/Host/CrmErp.Host/Migrations` targeting `ApplicationDbContext`.

To apply pending migrations to a target PostgreSQL database:
```bash
dotnet ef database update --project src/Host/CrmErp.Host/CrmErp.Host.csproj
```

Note: Automated startup migration execution and destructive operations like `EnsureDeleted()` are strictly disabled for production safety.

### C. Health Checks & Monitoring
- **Liveness & Readiness**: `GET /health` checks EF Core `ApplicationDbContext` database connection health.
- **Detailed Health Information**: `GET /api/v1/health` returns JSON system status and UTC timestamp.
- **Correlation ID**: Every HTTP request is assigned or preserves an `X-Correlation-ID` header, propagated through logs and error responses.

### D. Swagger / OpenAPI Documentation
- **Swagger UI**: Accessible at `/api/docs` when `OPENAPI_ENABLED=true` or in Development mode.
- **OpenAPI JSON**: Available at `/swagger/v1/swagger.json` or redirected from `/api/openapi.json`.
- **Security Schemes**: Supports JWT Bearer (`bearerAuth`) and Internal Service API Key (`X-Internal-API-Key`).

---

## 5. Local Setup & Verification

### Prerequisites
- .NET 8 SDK
- PostgreSQL 16 or Docker / Docker Compose

### Quick Start with Docker
```bash
# Build and run the gateway, database, API host, and integration layer
docker compose build
docker compose up -d

# Verify container status
docker compose ps
```

Endpoints when running via Docker Compose:
- **API Gateway (Nginx)**: `http://localhost:8080/`
- **Swagger Documentation**: `http://localhost:8080/api/docs`
- **Health Check**: `http://localhost:8080/health`

### Manual Execution & Testing
```bash
# Restore & Build (Debug & Release)
dotnet restore
dotnet build -c Release

# Execute full automated test suite
dotnet test -c Release

# Run API Host
dotnet run --project src/Host/CrmErp.Host/CrmErp.Host.csproj
```

---

## 6. End-to-End Workflow Architecture

The codebase natively enforces and tests the primary business pipeline:

```
Company -> Contact -> Lead -> Activity / Sales Note -> Lead Conversion -> Customer
  -> Opportunity -> Proposal -> Project -> Invoice -> Payment
```

Supporting workflows:
- **Customer Care**: Customer -> Support Ticket -> Ticket Comment / History
- **Platform Management**: Audit Logging -> Secure File Upload / Attachment -> System Settings
- **Reporting & Analytics**: Database-side aggregations for pre-sales pipelines, active deals, revenue, and support resolution metrics.
