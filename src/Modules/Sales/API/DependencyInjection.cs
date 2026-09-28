using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Application;

namespace Modules.Sales.API;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSalesApplication();
        // Register Sales API endpoints, controllers, request validators
        return services;
    }
}
