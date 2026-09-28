using System.Text;
using BuildingBlocks.Common;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// Add Building Blocks
builder.Services.AddBuildingBlocksCommon();
builder.Services.AddBuildingBlocksInfrastructure(builder.Configuration);

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "SuperSecretKeyForJwtTokenGeneration_MustBeAtLeast256BitsLong!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CrmErpApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CrmErpClients";

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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseUnifiedExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
