using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Application;

namespace Modules.Customer.API;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCustomerApplication();
        // Register Customer API endpoints, controllers, request validators
        return services;
    }
}
