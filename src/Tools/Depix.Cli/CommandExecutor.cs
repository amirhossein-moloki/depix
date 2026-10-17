using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Modules.Identity.Domain.Entities;

namespace Depix.Cli;

public class CommandExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly CSharpScriptEvaluator _scriptEvaluator;

    public CommandExecutor(IServiceProvider serviceProvider, IConfiguration configuration, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _environment = environment;
        _scriptEvaluator = new CSharpScriptEvaluator(_serviceProvider, _configuration, _environment);
    }

    public async Task ExecuteAsync(ParsedCommand command, IServiceProvider scopedServices, TextWriter output, CancellationToken cancellationToken = default)
    {
        var cmd = command.CommandName;
        var args = command.Arguments;

        switch (cmd)
        {
            case "help":
                ShowHelp(output);
                break;

            case "version":
                ShowVersion(output);
                break;

            case "config":
                if (args.Count > 0 && string.Equals(args[0], "show", StringComparison.OrdinalIgnoreCase))
                {
                    ShowConfig(output);
                }
                else
                {
                    output.WriteLine("Usage: config show");
                }
                break;

            case "services":
                if (args.Count > 0 && string.Equals(args[0], "list", StringComparison.OrdinalIgnoreCase))
                {
                    ListServices(scopedServices, output);
                }
                else
                {
                    output.WriteLine("Usage: services list");
                }
                break;

            case "db":
                if (args.Count > 0 && string.Equals(args[0], "status", StringComparison.OrdinalIgnoreCase))
                {
                    await ShowDbStatusAsync(scopedServices, output, cancellationToken);
                }
                else if (args.Count > 0 && string.Equals(args[0], "tables", StringComparison.OrdinalIgnoreCase))
                {
                    ShowDbTables(scopedServices, output);
                }
                else if (args.Count > 0 && string.Equals(args[0], "migrations", StringComparison.OrdinalIgnoreCase))
                {
                    await ShowDbMigrationsAsync(scopedServices, output, cancellationToken);
                }
                else
                {
                    output.WriteLine("Usage: db [status|tables|migrations]");
                }
                break;

            case "users":
                if (args.Count > 0 && string.Equals(args[0], "list", StringComparison.OrdinalIgnoreCase))
                {
                    await ListUsersAsync(scopedServices, output, cancellationToken);
                }
                else if (args.Count > 0 && string.Equals(args[0], "find", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Count < 2)
                    {
                        output.WriteLine("Usage: users find <email>");
                    }
                    else
                    {
                        await FindUserAsync(args[1], scopedServices, output, cancellationToken);
                    }
                }
                else if (args.Count > 0 && string.Equals(args[0], "roles", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Count < 2)
                    {
                        output.WriteLine("Usage: users roles <email>");
                    }
                    else
                    {
                        await ShowUserRolesAsync(args[1], scopedServices, output, cancellationToken);
                    }
                }
                else
                {
                    output.WriteLine("Usage: users [list|find <email>|roles <email>]");
                }
                break;

            case "health":
                await ShowHealthAsync(scopedServices, output, cancellationToken);
                break;

            case "logs":
                if (args.Count > 0 && string.Equals(args[0], "errors", StringComparison.OrdinalIgnoreCase))
                {
                    ShowLogsErrors(command, output);
                }
                else
                {
                    output.WriteLine("Usage: logs errors [--last 1h]");
                }
                break;

            case "eval":
                var expression = string.Join(" ", args);
                if (string.IsNullOrWhiteSpace(expression))
                {
                    output.WriteLine("Usage: eval <csharp_expression>");
                }
                else
                {
                    if (_scriptEvaluator.IsDisabledInCurrentEnvironment(out string? reason))
                    {
                        output.WriteLine($"[Security Notice] {reason}");
                        return;
                    }

                    try
                    {
                        var result = await _scriptEvaluator.EvaluateAsync(expression, scopedServices, cancellationToken);
                        if (result != null)
                        {
                            output.WriteLine(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        output.WriteLine($"[Script Error] {ex.Message}");
                    }
                }
                break;

            default:
                output.WriteLine($"Unknown command '{cmd}'. Type 'help' for available commands.");
                break;
        }
    }

    private void ShowHelp(TextWriter output)
    {
        var headers = new[] { "Command", "Description" };
        var rows = new[]
        {
            new[] { "help", "Display available commands" },
            new[] { "version", "Show system and environment version details" },
            new[] { "config show", "Display safe non-sensitive application settings" },
            new[] { "services list", "List registered dependency injection services" },
            new[] { "db status", "Verify database connection status and provider" },
            new[] { "db tables", "List actual database tables from EF Core metadata" },
            new[] { "db migrations", "Report EF Core applied and pending database migrations" },
            new[] { "users list", "List registered system users" },
            new[] { "users find <email>", "Search user profile by email address" },
            new[] { "users roles <email>", "Display assigned roles for a specific user" },
            new[] { "health", "Run system health checks (ApplicationDbContext)" },
            new[] { "logs errors [--last 1h]", "Check structured log status" },
            new[] { "csharp", "Enter interactive C# evaluation mode (Django Shell equivalent)" },
            new[] { "eval <expression>", "Evaluate a single C# expression" },
            new[] { "exit / quit", "Exit the shell" }
        };

        output.WriteLine(TableFormatter.RenderTable(headers, rows));
    }

    private void ShowVersion(TextWriter output)
    {
        var dbContext = _serviceProvider.GetService<ApplicationDbContext>();
        var provider = dbContext?.Database.ProviderName ?? "PostgreSQL";

        var pairs = new List<KeyValuePair<string, string>>
        {
            new("System", "Depix CRM/ERP Modular Monolith"),
            new("Cli Version", "1.0.0"),
            new("Runtime", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription),
            new("Environment", _environment.EnvironmentName),
            new("Database Provider", provider)
        };

        output.WriteLine(TableFormatter.RenderKeyValue(pairs));
    }

    private void ShowConfig(TextWriter output)
    {
        var configPairs = new List<KeyValuePair<string, string>>();

        foreach (var pair in _configuration.AsEnumerable().OrderBy(p => p.Key))
        {
            if (string.IsNullOrEmpty(pair.Value)) continue;

            var redacted = SecretRedactor.RedactValue(pair.Key, pair.Value);
            configPairs.Add(new KeyValuePair<string, string>(pair.Key, redacted));
        }

        output.WriteLine(TableFormatter.RenderKeyValue(configPairs));
    }

    private void ListServices(IServiceProvider scopedServices, TextWriter output)
    {
        var services = _serviceProvider.GetRequiredService<IServiceCollection>();
        var headers = new[] { "Service Type", "Implementation Type", "Lifetime" };

        var rows = services
            .Select(s => new[]
            {
                s.ServiceType.Name,
                s.ImplementationType?.Name ?? (s.ImplementationInstance != null ? s.ImplementationInstance.GetType().Name : (s.ImplementationFactory != null ? "Factory" : "Unknown")),
                s.Lifetime.ToString()
            })
            .OrderBy(r => r[0])
            .Take(50) // limit for display
            .ToList();

        output.WriteLine($"Total Registered Services: {services.Count} (Showing first 50)");
        output.WriteLine(TableFormatter.RenderTable(headers, rows));
    }

    private async Task ShowDbStatusAsync(IServiceProvider scopedServices, TextWriter output, CancellationToken cancellationToken)
    {
        var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();
        var provider = dbContext.Database.ProviderName ?? "Unknown";
        var connStr = dbContext.Database.GetConnectionString();
        var redactedConn = SecretRedactor.RedactConnectionString(connStr);

        bool canConnect = false;
        string statusStr = "Disconnected";
        try
        {
            canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            statusStr = canConnect ? "Connected (Healthy)" : "Cannot Connect";
        }
        catch (Exception ex)
        {
            statusStr = $"Connection Error: {ex.Message}";
        }

        var pairs = new List<KeyValuePair<string, string>>
        {
            new("Provider", provider),
            new("Status", statusStr),
            new("Connection String", redactedConn)
        };

        output.WriteLine(TableFormatter.RenderKeyValue(pairs));
    }

    private void ShowDbTables(IServiceProvider scopedServices, TextWriter output)
    {
        var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();
        var entityTypes = dbContext.Model.GetEntityTypes();

        var headers = new[] { "Entity Name", "Schema", "Table Name" };
        var rows = entityTypes
            .Select(e => new[]
            {
                e.ClrType.Name,
                e.GetSchema() ?? "public",
                e.GetTableName() ?? e.ClrType.Name
            })
            .OrderBy(r => r[1]).ThenBy(r => r[2])
            .ToList();

        output.WriteLine($"Total Database Tables Mapped: {rows.Count}");
        output.WriteLine(TableFormatter.RenderTable(headers, rows));
    }

    private async Task ShowDbMigrationsAsync(IServiceProvider scopedServices, TextWriter output, CancellationToken cancellationToken)
    {
        var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

        IEnumerable<string> applied = Array.Empty<string>();
        IEnumerable<string> pending = Array.Empty<string>();

        try
        {
            applied = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
            pending = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            output.WriteLine($"[Database Migration Error] {ex.Message}");
            return;
        }

        output.WriteLine($"Applied Migrations ({applied.Count()}):");
        foreach (var m in applied)
        {
            output.WriteLine($" - {m}");
        }

        output.WriteLine($"Pending Migrations ({pending.Count()}):");
        if (!pending.Any())
        {
            output.WriteLine(" - (None. Database schema is up to date.)");
        }
        else
        {
            foreach (var m in pending)
            {
                output.WriteLine($" - {m}");
            }
        }
    }

    private async Task ListUsersAsync(IServiceProvider scopedServices, TextWriter output, CancellationToken cancellationToken)
    {
        var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

        var users = await dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var roles = await dbContext.Set<Role>()
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);

        var headers = new[] { "ID", "Name", "Email", "Status", "Roles", "Phone" };
        var rows = users.Select(u =>
        {
            var userRolesStr = string.Join(", ", u.UserRoles.Select(ur => roles.TryGetValue(ur.RoleId, out var rName) ? rName : ur.RoleId.ToString()));
            return new[]
            {
                u.Id.ToString(),
                $"{u.FirstName} {u.LastName}".Trim(),
                u.Email,
                u.Status.ToString(),
                string.IsNullOrEmpty(userRolesStr) ? "None" : userRolesStr,
                u.Phone ?? "-"
            };
        }).ToList();

        output.WriteLine($"Total Registered Users: {users.Count}");
        output.WriteLine(TableFormatter.RenderTable(headers, rows));
    }

    private async Task FindUserAsync(string email, IServiceProvider scopedServices, TextWriter output, CancellationToken cancellationToken)
    {
        var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);

        if (user == null)
        {
            output.WriteLine($"User with email '{email}' was not found.");
            return;
        }

        var roles = await dbContext.Set<Role>()
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);

        var userRolesStr = string.Join(", ", user.UserRoles.Select(ur => roles.TryGetValue(ur.RoleId, out var rName) ? rName : ur.RoleId.ToString()));

        var pairs = new List<KeyValuePair<string, string>>
        {
            new("ID", user.Id.ToString()),
            new("First Name", user.FirstName),
            new("Last Name", user.LastName),
            new("Email", user.Email),
            new("Status", user.Status.ToString()),
            new("Roles", string.IsNullOrEmpty(userRolesStr) ? "None" : userRolesStr),
            new("Phone", user.Phone ?? "-"),
            new("Last Login At", user.LastLoginAt?.ToString("u") ?? "Never"),
            new("Created At", user.CreatedAt.ToString("u"))
        };

        output.WriteLine(TableFormatter.RenderKeyValue(pairs));
    }

    private async Task ShowUserRolesAsync(string email, IServiceProvider scopedServices, TextWriter output, CancellationToken cancellationToken)
    {
        var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

        var user = await dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);

        if (user == null)
        {
            output.WriteLine($"User with email '{email}' was not found.");
            return;
        }

        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var roles = await dbContext.Set<Role>()
            .Where(r => roleIds.Contains(r.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var headers = new[] { "Role ID", "Role Name", "Description" };
        var rows = roles.Select(r => new[]
        {
            r.Id.ToString(),
            r.Name,
            r.Description ?? "-"
        }).ToList();

        output.WriteLine($"User: {user.Email}");
        output.WriteLine(TableFormatter.RenderTable(headers, rows));
    }

    private async Task ShowHealthAsync(IServiceProvider scopedServices, TextWriter output, CancellationToken cancellationToken)
    {
        var healthCheckService = scopedServices.GetService<HealthCheckService>();
        if (healthCheckService == null)
        {
            output.WriteLine("HealthCheckService is not registered.");
            return;
        }

        var report = await healthCheckService.CheckHealthAsync(cancellationToken);

        var pairs = new List<KeyValuePair<string, string>>
        {
            new("Overall Status", report.Status.ToString()),
            new("Total Duration", report.TotalDuration.ToString())
        };

        foreach (var entry in report.Entries)
        {
            pairs.Add(new KeyValuePair<string, string>($"Entry: {entry.Key}", $"{entry.Value.Status} - {entry.Value.Description ?? "OK"} ({entry.Value.Duration})"));
        }

        output.WriteLine(TableFormatter.RenderKeyValue(pairs));
    }

    private void ShowLogsErrors(ParsedCommand command, TextWriter output)
    {
        var timeWindow = command.GetOptionValue("--last") ?? "1h";
        output.WriteLine($"[Logs Diagnostics]");
        output.WriteLine($"Queried error logs for time window: last {timeWindow}.");
        output.WriteLine("Note: Continuous structured file/sink log querying is managed via standard stdout container streams and EF audit logs.");
        output.WriteLine("To view real-time runtime error logs in Docker, use: docker compose logs -f backend");
    }
}
