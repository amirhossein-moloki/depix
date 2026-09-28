using Microsoft.Extensions.DependencyInjection;

namespace Modules.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        // Register Identity Application services, command handlers, query handlers, validators, etc.
        return services;
    }
}
