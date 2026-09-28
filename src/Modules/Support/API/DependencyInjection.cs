using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Support.Application;

namespace Modules.Support.API;

public static class DependencyInjection
{
    public static IServiceCollection AddSupportApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSupportApplication();
        // Register Support API endpoints, controllers, request validators
        return services;
    }
}
