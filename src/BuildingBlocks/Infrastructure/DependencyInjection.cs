using BuildingBlocks.Infrastructure.Persistence;
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

        return services;
    }
}
