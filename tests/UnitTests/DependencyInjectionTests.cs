using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.Events;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.CRM.Application;
using Modules.CRM.Infrastructure;
using Modules.Customer.Application;
using Modules.Customer.Infrastructure;
using Modules.Finance.Application;
using Modules.Finance.Infrastructure;
using Modules.Identity.Application;
using Modules.Identity.Infrastructure;
using Modules.Platform.Application;
using Modules.Platform.Infrastructure;
using Modules.Project.Application;
using Modules.Project.Infrastructure;
using Modules.Reporting.Application;
using Modules.Reporting.Infrastructure;
using Modules.Sales.Application;
using Modules.Sales.Infrastructure;
using Modules.Support.Application;
using Modules.Support.Infrastructure;
using Xunit;

namespace UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void ServiceCollection_ShouldResolveKeyInfrastructureAndApplicationServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:Database", "Host=localhost;Database=test_db;Username=postgres;Password=postgres" },
                { "ASPNETCORE_ENVIRONMENT", "Development" },
                { "Jwt:Secret", "Development_Placeholder_Jwt_Secret_Key_32_Bytes_Long_Minimum!" },
                { "Jwt:Issuer", "CrmErpApi" },
                { "Jwt:Audience", "CrmErpClients" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // Building blocks
        services.AddBuildingBlocksCommon();
        services.AddBuildingBlocksInfrastructure(configuration);

        // Replace DbContext with InMemory database for test environment
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("DiTestDb"));

        // Module registrations
        services.AddIdentityApplication();
        services.AddIdentityInfrastructure(configuration);

        services.AddCRMApplication();
        services.AddCRMInfrastructure(configuration);

        services.AddSalesApplication();
        services.AddSalesInfrastructure(configuration);

        services.AddCustomerApplication();
        services.AddCustomerInfrastructure(configuration);

        services.AddProjectApplication();
        services.AddProjectInfrastructure(configuration);

        services.AddFinanceApplication();
        services.AddFinanceInfrastructure(configuration);

        services.AddSupportApplication();
        services.AddSupportInfrastructure(configuration);

        services.AddPlatformApplication();
        services.AddPlatformInfrastructure(configuration);

        services.AddReportingApplication(configuration);
        services.AddReportingInfrastructure(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        Assert.NotNull(sp.GetRequiredService<IUnitOfWork>());
        Assert.NotNull(sp.GetRequiredService<IEventBus>());
        Assert.NotNull(sp.GetRequiredService<ApplicationDbContext>());
        Assert.NotNull(sp.GetRequiredService<ICustomerService>());
        Assert.NotNull(sp.GetRequiredService<IProjectService>());
        Assert.NotNull(sp.GetRequiredService<ICustomerContactService>());
    }
}
