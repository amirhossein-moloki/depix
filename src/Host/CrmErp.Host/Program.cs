using BuildingBlocks.Common;
using BuildingBlocks.Infrastructure;
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

// Add Building Blocks
builder.Services.AddBuildingBlocksCommon();
builder.Services.AddBuildingBlocksInfrastructure();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
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
