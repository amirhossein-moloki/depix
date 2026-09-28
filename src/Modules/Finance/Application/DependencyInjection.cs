using Microsoft.Extensions.DependencyInjection;

namespace Modules.Finance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        // Register Finance Application services, command handlers, query handlers, validators, etc.
        return services;
    }
}
