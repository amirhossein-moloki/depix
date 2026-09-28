using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Finance.Application;

namespace Modules.Finance.API;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFinanceApplication();
        // Register Finance API endpoints, controllers, request validators
        return services;
    }
}
