# Depix Shell (CLI) — Developer & Administrative Shell

`Depix Shell` is an interactive developer and administrative shell for Depix CRM/ERP (ASP.NET Core Modular Monolith), providing functionality equivalent to Django Shell.

It directly reuses the application's real dependency injection container, EF Core `ApplicationDbContext`, configuration sources, and domain services without running an HTTP server or exposing any network ports.

---

## 1. Quick Start

### Launch Interactive REPL
```bash
dotnet run --project src/Tools/Depix.Cli
```

### Execute Single Non-Interactive Command
```bash
dotnet run --project src/Tools/Depix.Cli -- db status
dotnet run --project src/Tools/Depix.Cli -- users list
dotnet run --project src/Tools/Depix.Cli -- eval "dbContext.Model.GetEntityTypes().Count()"
```

---

## 2. Built-in Commands

### General Commands
- `help`: Display all available commands and brief usage.
- `version`: Display system, CLI version, .NET runtime, environment, and database provider.
- `config show`: Display safe application settings (automatically masks secrets, tokens, passwords, and connection credentials).
- `services list`: List registered dependency injection services and lifetimes.

### Database Commands
- `db status`: Check database connectivity status, provider, and target connection details (credentials redacted).
- `db tables`: List all 55 mapped EF Core entity tables and schema mappings across modules.
- `db migrations`: Report applied and pending EF Core database migrations.

### User Management Commands
- `users list`: List system users with name, email, status, phone, and assigned roles (password hashes and refresh tokens redacted).
- `users find <email>`: Search detailed user profile by email address.
- `users roles <email>`: Display assigned roles and descriptions for a user.

### Diagnostics & Health Commands
- `health`: Execute registered health checks (`ApplicationDbContext`).
- `logs errors [--last 1h]`: Inspect structured logging status.

---

## 3. Interactive C# Scripting Evaluation (Django Shell Mode)

Depix Shell includes Roslyn-powered interactive C# evaluation.

### Enter C# REPL Mode
Type `csharp` inside the interactive shell:

```text
depix> csharp
Entering C# script mode. Multi-statement variables and LINQ are supported.
Type 'exit' to leave C# script mode.

depix (csharp)> var users = await dbContext.Set<User>().Take(10).ToListAsync();
depix (csharp)> users.Count
1
depix (csharp)> users.First().Email
admin@depix.ir
depix (csharp)> exit
Exited C# script mode.
```

### Single-Line Evaluation
You can also evaluate C# expressions directly using `eval`:

```text
depix> eval dbContext.Model.GetEntityTypes().Count()
55
```

### Available Shell Context Objects
Within C# scripting mode, the following context objects are automatically exposed:

| Variable | Type | Description |
| :--- | :--- | :--- |
| `dbContext` | `ApplicationDbContext` | Configured EF Core DbContext with soft-delete filters and module mappings |
| `services` | `IServiceProvider` | Current scoped dependency injection service provider |
| `configuration` | `IConfiguration` | Application configuration root |
| `environment` | `IHostEnvironment` | Host environment details (`Development`, `Production`, etc.) |

Default imports include all domain entity namespaces across `Identity`, `CRM`, `Sales`, `Customer`, `Project`, `Finance`, `Support`, and `Platform`.

---

## 4. Production Safety Controls

Depix Shell is designed with administrative security and data safety controls:

1. **No HTTP Endpoint**: The CLI never listens on any HTTP port or starts a web server.
2. **Secret Redaction**: Passwords, connection string credentials, JWT secret keys, and tokens are automatically masked in output (`config show`, `db status`).
3. **Production C# Scripting Guard**: Arbitrary C# evaluation (`csharp` and `eval`) is disabled by default in `Production` environment (`ASPNETCORE_ENVIRONMENT=Production`).
   To override this guard in production for authorized emergency maintenance, set the environment variable:
   ```bash
   DEPIX_ALLOW_PROD_SCRIPTING=true
   ```

---

## 5. Running in Docker Compose

To run Depix Shell inside the running backend container:

```bash
docker compose exec backend dotnet run --project src/Tools/Depix.Cli
```

Or execute non-interactive commands:

```bash
docker compose exec backend dotnet run --project src/Tools/Depix.Cli -- db status
```
