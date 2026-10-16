using BuildingBlocks.Application.Events;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Infrastructure.Events;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        var env = configuration["ASPNETCORE_ENVIRONMENT"];
        var isProduction = string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("<DATABASE_PASSWORD>"))
        {
            if (isProduction)
            {
                throw new InvalidOperationException("ConnectionStrings:Database configuration is required in Production.");
            }
            connectionString = "Host=localhost;Database=crm_erp_db;Username=postgres;Password=postgres";
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Configure Data Protection Key Persistence to avoid unpersisted key ring warnings in containers
        var dataProtectionPath = configuration["DataProtection:Path"] ?? "/app/dpkeys";
        if (!Directory.Exists(dataProtectionPath))
        {
            try
            {
                Directory.CreateDirectory(dataProtectionPath);
            }
            catch
            {
                // Fallback to local temp directory if default path is inaccessible
                dataProtectionPath = Path.Combine(Path.GetTempPath(), "crmerp-dpkeys");
                Directory.CreateDirectory(dataProtectionPath);
            }
        }

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
            .SetApplicationName("CrmErp");

        services.AddScoped<IEventBus, InMemoryEventBus>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
