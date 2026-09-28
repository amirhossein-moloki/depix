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
1. **Identity Module**: Handles user management, roles, claims, permissions, authentication (JWT/OAuth), and authorization policies.
2. **CRM Module**: Focuses on **pre-sales lead nurture and acquisition** (Leads, Companies, Contacts, Lead Research, Sales Notes, Lead Activities).
3. **Sales Module**: Handles **sales pipeline conversion** (Opportunities, Proposals, Proposal Items, Stages, Deal Tracking). Hands off qualified leads to Customer and Finance modules.
4. **Customer Module**: Manages **converted paying customers** and customer profiles. Clear separation between pre-sale leads (CRM) and active customer relationships (Customer).
5. **Project Module**: Core operational module for delivery. Handles Website and Software Projects, Technical Requirements, Tech Stacks, Git Repositories, Deployments, and Technical Documentation.
6. **Finance Module**: Handles financial tracking, Contracts, Payment Schedules/Items, and Transaction records. Interacts with Customer and Sales modules asynchronously via events.
7. **Support Module**: Manages post-delivery services, Support Plans, Service Level Agreements (SLAs), and Support Tickets.
8. **Platform Module**: Provides shared internal capabilities including File Management, Internal Operational Tasks, Audit Logs, and Notification Dispatchers.

### C. Communication Between Modules
- **Rule**: Direct project-to-project references or entity sharing between modules is strictly forbidden.
- **In-Memory Event Bus**: Modules communicate via domain events / integration events (`IEventBus`) for eventual consistency.
- **Foreign Keys**: Primary keys (e.g. `Guid CustomerId`) are stored as primitive scalar values across module boundaries rather than navigational Entity properties.

---

## 2. Solution Structure

```
src/
├── BuildingBlocks/
│   ├── Common/                  # DI extensions, DateTime providers, cross-cutting helpers
│   ├── Domain/                  # Base Entity, AggregateRoot, ValueObject, IDomainEvent
│   ├── Application/             # CQRS interfaces (ICommand, IQuery), IEventBus, Validation
│   ├── Infrastructure/          # Shared EF Core base configs, Outbox, Interceptors
│   └── SharedKernel/            # Result<T>, Error, PagedResult<T> primitives
│
├── Modules/
│   ├── Identity/
│   │   ├── Domain/              # User, Role, UserRole
│   │   ├── Application/         # Auth use cases, Commands, Queries, DTOs
│   │   ├── Infrastructure/      # Identity DbContext, EF Core mappings
│   │   └── API/                 # Auth & User Controllers, Endpoints
│   │
│   ├── CRM/                     # Company, Contact, Lead, LeadResearch, Activity, SalesNote
│   ├── Sales/                   # Opportunity, Proposal, ProposalItem, Stages
│   ├── Customer/                # Customer profile & lifecycle
│   ├── Project/                 # Project, Requirement, TechStack, Repository, Deployment
│   ├── Finance/                 # Contract, ContractPayment, Transaction
│   ├── Support/                 # SupportPlan, Ticket
│   └── Platform/                # File, Internal Task, AuditLog
│
└── Host/
    └── CrmErp.Host/             # ASP.NET Core Web API Host & Composition Root

tests/
└── CrmErp.ArchitectureTests/    # NetArchTest Clean Architecture verification test suite
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

## 4. Architectural Verification

Automated architecture tests in `tests/CrmErp.ArchitectureTests` enforce these rules on every build using `NetArchTest.Rules`:
- Domain layers have zero dependencies on outer layers.
- Application layers do not depend on Infrastructure, API, or Host.
- API layers do not depend directly on Infrastructure.
- Modules do not reference other modules directly.

Run tests:
```bash
dotnet test
```

---

## 5. Deployment & Local Development

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose (optional for database)

### Quick Start with Docker
```bash
# Start PostgreSQL database and API host
docker-compose up --build
```
API Root Endpoint: `http://localhost:8080/`
Swagger UI: `http://localhost:8080/swagger`

### Manual Execution
```bash
# Restore & Build
dotnet restore
dotnet build

# Run API Host
dotnet run --project src/Host/CrmErp.Host/CrmErp.Host.csproj
```

---

## 6. PostgreSQL Database & Migration Strategy
Each module operates on its own DbContext and isolated database schema within PostgreSQL:
- `identity` schema
- `crm` schema
- `sales` schema
- `customer` schema
- `project` schema
- `finance` schema
- `support` schema
- `platform` schema

This schema separation facilitates future microservice extraction if scale requirements demand physical database decoupling.
