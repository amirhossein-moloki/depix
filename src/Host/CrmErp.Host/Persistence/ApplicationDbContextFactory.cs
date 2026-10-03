using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CrmErp.Host.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var env = configuration["ASPNETCORE_ENVIRONMENT"];
            if (string.Equals(env, "Production", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("ConnectionStrings:Database configuration is required in Production.");
            }
            connectionString = "Host=localhost;Database=crm_erp_db;Username=postgres;Password=postgres";
        }

        // Preload module infrastructure assemblies into AppDomain
        _ = Modules.Identity.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.CRM.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Sales.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Customer.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Project.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Finance.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Support.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Platform.Infrastructure.AssemblyReference.Assembly;

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ApplicationDbContextFactory).Assembly.FullName));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
