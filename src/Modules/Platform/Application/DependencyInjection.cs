using Microsoft.Extensions.DependencyInjection;

namespace Modules.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApplication(this IServiceCollection services)
    {
        // Register Platform Application services, command handlers, query handlers, validators, etc.
        return services;
    }
}
