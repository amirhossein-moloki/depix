using System.Text;
using System.Threading.RateLimiting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using BuildingBlocks.Common;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Modules.CRM.API;
using Modules.CRM.Infrastructure;
using Modules.Customer.API;
using Modules.Customer.Infrastructure;
using Modules.Finance.API;
using Modules.Finance.Infrastructure;
using Modules.Identity.API;
using Modules.Identity.Infrastructure;
using Modules.Platform.API;
using Modules.Platform.Infrastructure;
using Modules.Project.API;
using Modules.Project.Infrastructure;
using Modules.Reporting.API;
using Modules.Reporting.Infrastructure;
using Modules.Sales.API;
using Modules.Sales.Infrastructure;
using Modules.Support.API;
using Modules.Support.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog Logger
var logDirectory = builder.Configuration["LOG_DIR"] ?? builder.Configuration["Logging:LogDirectory"] ?? "logs";
var logFilePath = Path.Combine(logDirectory, "depix-.json");
int retainedFiles = int.TryParse(builder.Configuration["Logging:RetainedFileCountLimit"], out var rCount) ? rCount : 30;
long fileSizeLimit = long.TryParse(builder.Configuration["Logging:FileSizeLimitBytes"], out var fSize) ? fSize : 10_485_760;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Depix.Api")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(new CompactJsonFormatter())
    .WriteTo.File(
        formatter: new CompactJsonFormatter(),
        path: logFilePath,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: retainedFiles,
        fileSizeLimitBytes: fileSizeLimit,
        rollOnFileSizeLimit: true,
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

// Preload module infrastructure assemblies into AppDomain for EF Core configuration scanning
_ = Modules.Identity.Infrastructure.AssemblyReference.Assembly;
_ = Modules.CRM.Infrastructure.AssemblyReference.Assembly;
_ = Modules.Sales.Infrastructure.AssemblyReference.Assembly;
_ = Modules.Customer.Infrastructure.AssemblyReference.Assembly;
_ = Modules.Project.Infrastructure.AssemblyReference.Assembly;
_ = Modules.Finance.Infrastructure.AssemblyReference.Assembly;
_ = Modules.Support.Infrastructure.AssemblyReference.Assembly;
_ = Modules.Platform.Infrastructure.AssemblyReference.Assembly;
_ = Modules.Reporting.Infrastructure.AssemblyReference.Assembly;

// Add Building Blocks
builder.Services.AddBuildingBlocksCommon();
builder.Services.AddBuildingBlocksInfrastructure(builder.Configuration);

// Configure Forwarded Headers for Reverse Proxy support (Nginx / Caddy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure CORS Policy
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => false);
        }
    });
});

// Add Database Health Check
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Configure Rate Limiting Foundation
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (builder.Environment.IsProduction())
{
    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32 || jwtSecret.Contains("PLACEHOLDER"))
    {
        throw new InvalidOperationException("Jwt:Secret must be configured and be at least 32 characters long in Production.");
    }
    if (string.IsNullOrWhiteSpace(jwtIssuer))
    {
        throw new InvalidOperationException("Jwt:Issuer must be configured in Production.");
    }
    if (string.IsNullOrWhiteSpace(jwtAudience))
    {
        throw new InvalidOperationException("Jwt:Audience must be configured in Production.");
    }
}
else
{
    if (string.IsNullOrWhiteSpace(jwtSecret))
    {
        jwtSecret = "Development_Placeholder_Jwt_Secret_Key_32_Bytes_Long_Minimum!";
    }
    jwtIssuer ??= "CrmErpApi";
    jwtAudience ??= "CrmErpClients";
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// Add Application Modules
builder.Services.AddIdentityApi(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);

builder.Services.AddCRMApi(builder.Configuration);
builder.Services.AddCRMInfrastructure(builder.Configuration);

builder.Services.AddSalesApi(builder.Configuration);
builder.Services.AddSalesInfrastructure(builder.Configuration);

builder.Services.AddCustomerApi(builder.Configuration);
builder.Services.AddCustomerInfrastructure(builder.Configuration);

builder.Services.AddProjectApi(builder.Configuration);
builder.Services.AddProjectInfrastructure(builder.Configuration);

builder.Services.AddFinanceApi(builder.Configuration);
builder.Services.AddFinanceInfrastructure(builder.Configuration);

builder.Services.AddSupportApi(builder.Configuration);
builder.Services.AddSupportInfrastructure(builder.Configuration);

builder.Services.AddPlatformApi(builder.Configuration);
builder.Services.AddPlatformInfrastructure(builder.Configuration);

builder.Services.AddReportingApi(builder.Configuration);
builder.Services.AddReportingInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Depix API",
        Version = "1.0.0",
        Description = "Unified API for Depix CRM/ERP Operations"
    });

    c.AddSecurityDefinition("bearerAuth", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "bearerAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Check for CLI argument: create-admin
if (args.Length > 0 && args[0].Equals("create-admin", StringComparison.OrdinalIgnoreCase))
{
    var tempApp = builder.Build();
    using (var scope = tempApp.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var hasher = services.GetRequiredService<Modules.Identity.Application.Common.Interfaces.IPasswordHasherService>();

        logger.LogInformation("Running CLI command: create-admin");

        string email = string.Empty;
        string password = string.Empty;
        string firstName = "Admin";
        string lastName = "System";

        // Extract CLI arguments if provided e.g. --email admin@domain.com --password secret
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i].Equals("--email", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                email = args[i + 1];
                i++;
            }
            else if (args[i].Equals("--password", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                password = args[i + 1];
                i++;
            }
            else if (args[i].Equals("--firstname", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                firstName = args[i + 1];
                i++;
            }
            else if (args[i].Equals("--lastname", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                lastName = args[i + 1];
                i++;
            }
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Write("Enter Admin Email: ");
            email = Console.ReadLine()?.Trim() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.Write("Enter Admin Password: ");
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    sb.Append(key.KeyChar);
                }
            }
            password = sb.ToString();
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            Console.Error.WriteLine("Error: Admin email and password cannot be empty.");
            Environment.Exit(1);
            return;
        }

        await dbContext.Database.MigrateAsync();

        // System Permissions Seed Definitions
        var systemPermissions = new (string Code, string Name, string Module)[]
        {
            // CRM
            ("CRM.Lead.Create", "Create Lead", "CRM"),
            ("CRM.Lead.Read", "Read Lead", "CRM"),
            ("CRM.Lead.Update", "Update Lead", "CRM"),
            ("CRM.Lead.Assign", "Assign Lead", "CRM"),
            ("CRM.Lead.StatusChange", "Change Lead Status", "CRM"),
            ("CRM.Lead.Qualify", "Qualify/Disqualify Lead", "CRM"),
            ("CRM.Lead.Convert", "Convert Lead", "CRM"),
            ("CRM.Lead.Delete", "Delete Lead", "CRM"),

            ("CRM.Company.Create", "Create Company", "CRM"),
            ("CRM.Company.Read", "Read Company", "CRM"),
            ("CRM.Company.Update", "Update Company", "CRM"),
            ("CRM.Company.Delete", "Delete Company", "CRM"),

            ("CRM.Contact.Create", "Create Contact", "CRM"),
            ("CRM.Contact.Read", "Read Contact", "CRM"),
            ("CRM.Contact.Update", "Update Contact", "CRM"),
            ("CRM.Contact.Delete", "Delete Contact", "CRM"),

            ("CRM.Activity.Create", "Create Activity", "CRM"),
            ("CRM.Activity.Read", "Read Activity", "CRM"),
            ("CRM.Activity.Update", "Update Activity", "CRM"),
            ("CRM.Activity.Delete", "Delete Activity", "CRM"),

            ("CRM.SalesNote.Create", "Create Sales Note", "CRM"),
            ("CRM.SalesNote.Read", "Read Sales Note", "CRM"),
            ("CRM.SalesNote.Update", "Update Sales Note", "CRM"),
            ("CRM.SalesNote.Delete", "Delete Sales Note", "CRM"),

            // Customer
            ("Customer.Create", "Create Customer", "Customer"),
            ("Customer.Read", "Read Customer", "Customer"),
            ("Customer.Update", "Update Customer", "Customer"),
            ("Customer.StatusChange", "Change Customer Status", "Customer"),
            ("Customer.Assign", "Assign Customer", "Customer"),
            ("Customer.Delete", "Delete Customer", "Customer"),

            // Sales
            ("Sales.Opportunity.Create", "Create Opportunity", "Sales"),
            ("Sales.Opportunity.Read", "Read Opportunity", "Sales"),
            ("Sales.Opportunity.Update", "Update Opportunity", "Sales"),
            ("Sales.Opportunity.Assign", "Assign Opportunity", "Sales"),
            ("Sales.Opportunity.StageChange", "Change Opportunity Stage", "Sales"),
            ("Sales.Opportunity.Close", "Close Opportunity", "Sales"),
            ("Sales.Opportunity.Delete", "Delete Opportunity", "Sales"),

            ("Sales.Proposal.Create", "Create Proposal", "Sales"),
            ("Sales.Proposal.Read", "Read Proposal", "Sales"),
            ("Sales.Proposal.Update", "Update Proposal", "Sales"),
            ("Sales.Proposal.StatusChange", "Change Proposal Status", "Sales"),
            ("Sales.Proposal.Delete", "Delete Proposal", "Sales"),

            // Project
            ("Project.Create", "Create Project", "Project"),
            ("Project.Read", "Read Project", "Project"),
            ("Project.Update", "Update Project", "Project"),
            ("Project.StatusChange", "Change Project Status", "Project"),
            ("Project.Delete", "Delete Project", "Project"),
            ("Project.Repository.Manage", "Manage Repositories", "Project"),
            ("Project.Deployment.Manage", "Manage Deployments", "Project"),

            // Finance
            ("Finance.Invoice.Create", "Create Invoice", "Finance"),
            ("Finance.Invoice.Read", "Read Invoice", "Finance"),
            ("Finance.Invoice.Update", "Update Invoice", "Finance"),
            ("Finance.Invoice.Issue", "Issue Invoice", "Finance"),
            ("Finance.Invoice.Cancel", "Cancel Invoice", "Finance"),
            ("Finance.Invoice.Delete", "Delete Invoice", "Finance"),
            ("Finance.Payment.Create", "Create Payment", "Finance"),
            ("Finance.Payment.Read", "Read Payment", "Finance"),

            // Support
            ("Support.Ticket.Create", "Create Ticket", "Support"),
            ("Support.Ticket.Read", "Read Ticket", "Support"),
            ("Support.Ticket.Update", "Update Ticket", "Support"),
            ("Support.Ticket.Assign", "Assign Ticket", "Support"),
            ("Support.Ticket.StatusChange", "Change Ticket Status", "Support"),
            ("Support.Ticket.Resolve", "Resolve Ticket", "Support"),
            ("Support.Ticket.Close", "Close Ticket", "Support"),
            ("Support.Ticket.Delete", "Delete Ticket", "Support"),
            ("Support.Comment.Create", "Create Ticket Comment", "Support"),
            ("Support.Comment.Read", "Read Ticket Comment", "Support"),

            // Platform
            ("Platform.Task.Create", "Create Task", "Platform"),
            ("Platform.Task.Read", "Read Task", "Platform"),
            ("Platform.Task.Update", "Update Task", "Platform"),
            ("Platform.Task.Delete", "Delete Task", "Platform"),

            ("Platform.File.Create", "Upload File", "Platform"),
            ("Platform.File.Read", "Read File", "Platform"),
            ("Platform.File.Delete", "Delete File", "Platform"),

            ("Platform.Audit.Read", "Read Audit Logs", "Platform"),

            ("Platform.Setting.Read", "Read System Settings", "Platform"),
            ("Platform.Setting.Write", "Write System Settings", "Platform"),
            ("Platform.Setting.Delete", "Delete System Settings", "Platform"),

            // Reporting
            ("Reporting.Read", "Read Summary Report", "Reporting"),
            ("Reporting.CRM.Read", "Read CRM Report", "Reporting"),
            ("Reporting.Sales.Read", "Read Sales Report", "Reporting"),
            ("Reporting.Customer.Read", "Read Customer Report", "Reporting"),
            ("Reporting.Project.Read", "Read Project Report", "Reporting"),
            ("Reporting.Finance.Read", "Read Finance Report", "Reporting"),
            ("Reporting.Support.Read", "Read Support Report", "Reporting")
        };

        // Seed permissions in database
        var existingPermissions = await dbContext.Set<Modules.Identity.Domain.Entities.Permission>().ToListAsync();
        var allPermissionEntities = new List<Modules.Identity.Domain.Entities.Permission>(existingPermissions);

        foreach (var p in systemPermissions)
        {
            var existing = existingPermissions.FirstOrDefault(ep => ep.Code.Equals(p.Code, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                var newPerm = Modules.Identity.Domain.Entities.Permission.Create(p.Code, p.Name, p.Module);
                await dbContext.Set<Modules.Identity.Domain.Entities.Permission>().AddAsync(newPerm);
                allPermissionEntities.Add(newPerm);
            }
        }
        await dbContext.SaveChangesAsync();

        // Ensure Admin Role exists
        var adminRole = await dbContext.Set<Modules.Identity.Domain.Entities.Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name.ToLower() == "admin");

        if (adminRole == null)
        {
            adminRole = Modules.Identity.Domain.Entities.Role.Create("Admin", "System Administrator Role");
            await dbContext.Set<Modules.Identity.Domain.Entities.Role>().AddAsync(adminRole);
            await dbContext.SaveChangesAsync();
        }

        // Attach all permissions to Admin role
        foreach (var perm in allPermissionEntities)
        {
            adminRole.AddPermission(perm.Id);
        }
        await dbContext.SaveChangesAsync();

        var existingUser = await dbContext.Set<Modules.Identity.Domain.Entities.User>()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (existingUser != null)
        {
            existingUser.AddRole(adminRole.Id);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Admin user with email '{email}' already exists. Assigned all permissions to Admin role.");
            Environment.Exit(0);
            return;
        }

        var hashedPassword = hasher.HashPassword(password);
        var adminUser = Modules.Identity.Domain.Entities.User.Create(
            firstName,
            lastName,
            email,
            hashedPassword,
            status: Modules.Identity.Domain.Entities.UserStatus.Active
        );

        adminUser.AddRole(adminRole.Id);
        await dbContext.Set<Modules.Identity.Domain.Entities.User>().AddAsync(adminUser);
        await dbContext.SaveChangesAsync();

        Console.WriteLine($"Admin user '{email}' created successfully with active status and full permissions.");
        Environment.Exit(0);
        return;
    }
}

// Check for CLI argument: reset-password
if (args.Length > 0 && args[0].Equals("reset-password", StringComparison.OrdinalIgnoreCase))
{
    var tempApp = builder.Build();
    using (var scope = tempApp.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var hasher = services.GetRequiredService<Modules.Identity.Application.Common.Interfaces.IPasswordHasherService>();

        logger.LogInformation("Running CLI command: reset-password");

        string email = string.Empty;
        string password = string.Empty;

        // Extract CLI arguments if provided e.g. --email user@domain.com --password secret OR --new-password secret
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i].Equals("--email", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                email = args[i + 1];
                i++;
            }
            else if ((args[i].Equals("--password", StringComparison.OrdinalIgnoreCase) || args[i].Equals("--new-password", StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length)
            {
                password = args[i + 1];
                i++;
            }
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Write("Enter User Email: ");
            email = Console.ReadLine()?.Trim() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.Write("Enter New Password: ");
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    sb.Append(key.KeyChar);
                }
            }
            password = sb.ToString();
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            Console.Error.WriteLine("Error: User email and new password cannot be empty.");
            Environment.Exit(1);
            return;
        }

        await dbContext.Database.MigrateAsync();

        var existingUser = await dbContext.Set<Modules.Identity.Domain.Entities.User>()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (existingUser == null)
        {
            Console.Error.WriteLine($"Error: User with email '{email}' not found.");
            Environment.Exit(1);
            return;
        }

        var hashedPassword = hasher.HashPassword(password);
        existingUser.UpdatePassword(hashedPassword);

        // Revoke active refresh tokens upon password reset
        foreach (var token in existingUser.RefreshTokens.Where(rt => rt.IsActive))
        {
            token.Revoke();
        }

        await dbContext.SaveChangesAsync();

        Console.WriteLine($"Password for user '{email}' reset successfully.");
        Environment.Exit(0);
        return;
    }
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Applying EF Core database migrations on startup...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying EF Core database migrations on startup.");
        throw;
    }
}

app.UseForwardedHeaders();
app.UseCorrelationId();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;
        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;
        return LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);

        // Strip sensitive query parameters if present
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var queryString = httpContext.Request.QueryString.Value;
        if (!string.IsNullOrEmpty(queryString))
        {
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString);
            var sanitizedQuery = new List<string>();
            foreach (var kvp in query)
            {
                var key = kvp.Key;
                if (key.Equals("token", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("access_token", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("secret", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("password", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("key", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("api_key", StringComparison.OrdinalIgnoreCase))
                {
                    sanitizedQuery.Add($"{key}=[REDACTED]");
                }
                else
                {
                    sanitizedQuery.Add($"{key}={kvp.Value}");
                }
            }
            path += "?" + string.Join("&", sanitizedQuery);
        }
        diagnosticContext.Set("RequestPath", path);
        diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);

        if (httpContext.Response.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            diagnosticContext.Set("CorrelationId", correlationId.ToString());
        }

        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                diagnosticContext.Set("UserId", userId);
            }
        }
    };
});

app.UseUnifiedExceptionHandler();

var openApiEnabled = builder.Configuration.GetValue<bool>("OPENAPI_ENABLED", true);
if (app.Environment.IsDevelopment() || openApiEnabled)
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Depix API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("DefaultCorsPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/api/v1/health", () => Results.Ok(new { Status = "Healthy", System = "Depix API", Timestamp = DateTime.UtcNow }));

app.MapGet("/api/openapi.json", (HttpContext context) =>
{
    context.Response.Redirect("/swagger/v1/swagger.json", permanent: false);
});

app.MapGet("/api/docs", (HttpContext context) =>
{
    context.Response.Redirect("/swagger/index.html", permanent: false);
});

app.MapGet("/", () => Results.Ok(new
{
    System = "CRM/ERP Web Development Modular Monolith API",
    Status = "Healthy",
    Version = "1.0.0",
    Modules = new[]
    {
        "Identity",
        "CRM",
        "Sales",
        "Customer",
        "Project",
        "Finance",
        "Support",
        "Platform"
    }
}));

app.Run();
