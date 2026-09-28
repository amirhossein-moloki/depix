using Microsoft.Extensions.DependencyInjection;

namespace Modules.Support.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSupportApplication(this IServiceCollection services)
    {
        // Register Support Application services, command handlers, query handlers, validators, etc.
        return services;
    }
}
