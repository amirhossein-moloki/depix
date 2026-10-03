using System.Text;
using System.Threading.RateLimiting;
using BuildingBlocks.Common;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
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
        Description = "Unified API for Depix Commerce, CMS, and CRM/ERP Operations"
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

    c.AddSecurityDefinition("internalApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Internal-API-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Internal Service-to-Service API Key for integration endpoints."
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

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCorrelationId();
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
        c.RoutePrefix = "api/docs";
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
