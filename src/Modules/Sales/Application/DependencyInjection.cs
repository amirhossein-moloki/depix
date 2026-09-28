using Microsoft.Extensions.DependencyInjection;

namespace Modules.Sales.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesApplication(this IServiceCollection services)
    {
        // Register Sales Application services, command handlers, query handlers, validators, etc.
        return services;
    }
}
