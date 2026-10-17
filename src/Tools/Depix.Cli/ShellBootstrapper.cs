using BuildingBlocks.Common;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

namespace Depix.Cli;

public static class ShellBootstrapper
{
    public static (IServiceProvider Services, IConfiguration Configuration, IHostEnvironment Environment) BuildServiceProvider(
        string[]? args = null,
        IConfiguration? existingConfig = null,
        Action<IServiceCollection>? configureExtraServices = null)
    {
        // 1. Preload module infrastructure assemblies for EF Core configuration scanning
        _ = Modules.Identity.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.CRM.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Sales.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Customer.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Project.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Finance.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Support.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Platform.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Reporting.Infrastructure.AssemblyReference.Assembly;

        // 2. Build Configuration
        IConfiguration configuration;
        if (existingConfig != null)
        {
            configuration = existingConfig;
        }
        else
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var configBuilder = new ConfigurationBuilder();

            // Try current directory and host directory for appsettings.json
            var basePath = Directory.GetCurrentDirectory();
            var hostPath = Path.Combine(basePath, "src", "Host", "CrmErp.Host");
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")) && Directory.Exists(hostPath))
            {
                basePath = hostPath;
            }

            configBuilder.SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            if (args != null && args.Length > 0)
            {
                configBuilder.AddCommandLine(args);
            }

            configuration = configBuilder.Build();
        }

        // 3. Create HostEnvironment
        var env = new CliHostEnvironment(
            environmentName: configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
            applicationName: "Depix.Cli",
            contentRootPath: Directory.GetCurrentDirectory());

        // 4. Configure ServiceCollection
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(env);

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        // Building Blocks
        services.AddBuildingBlocksCommon();
        services.AddBuildingBlocksInfrastructure(configuration);

        // Database Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        // Application Modules
        services.AddIdentityApi(configuration);
        services.AddIdentityInfrastructure(configuration);

        services.AddCRMApi(configuration);
        services.AddCRMInfrastructure(configuration);

        services.AddSalesApi(configuration);
        services.AddSalesInfrastructure(configuration);

        services.AddCustomerApi(configuration);
        services.AddCustomerInfrastructure(configuration);

        services.AddProjectApi(configuration);
        services.AddProjectInfrastructure(configuration);

        services.AddFinanceApi(configuration);
        services.AddFinanceInfrastructure(configuration);

        services.AddSupportApi(configuration);
        services.AddSupportInfrastructure(configuration);

        services.AddPlatformApi(configuration);
        services.AddPlatformInfrastructure(configuration);

        services.AddReportingApi(configuration);
        services.AddReportingInfrastructure(configuration);

        configureExtraServices?.Invoke(services);

        // Register ServiceCollection instance in DI for services list command
        services.AddSingleton<IServiceCollection>(services);

        var serviceProvider = services.BuildServiceProvider();
        return (serviceProvider, configuration, env);
    }
}

public class CliHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; }
    public string ApplicationName { get; set; }
    public string ContentRootPath { get; set; }
    public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }

    public CliHostEnvironment(string environmentName, string applicationName, string contentRootPath)
    {
        EnvironmentName = environmentName;
        ApplicationName = applicationName;
        ContentRootPath = contentRootPath;
        ContentRootFileProvider = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
