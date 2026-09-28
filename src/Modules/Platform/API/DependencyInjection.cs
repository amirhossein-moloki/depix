using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Platform.Application;

namespace Modules.Platform.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPlatformApplication();
        // Register Platform API endpoints, controllers, request validators
        return services;
    }
}
