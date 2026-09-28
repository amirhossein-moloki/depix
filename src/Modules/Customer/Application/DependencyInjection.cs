using Microsoft.Extensions.DependencyInjection;

namespace Modules.Customer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerApplication(this IServiceCollection services)
    {
        // Register Customer Application services, command handlers, query handlers, validators, etc.
        return services;
    }
}
