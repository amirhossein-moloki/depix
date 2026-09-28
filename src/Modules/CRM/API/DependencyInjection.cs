using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.CRM.Application;

namespace Modules.CRM.API;

public static class DependencyInjection
{
    public static IServiceCollection AddCRMApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCRMApplication();
        // Register CRM API endpoints, controllers, request validators
        return services;
    }
}
