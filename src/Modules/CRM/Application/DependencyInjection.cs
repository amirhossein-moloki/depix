using Microsoft.Extensions.DependencyInjection;

namespace Modules.CRM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCRMApplication(this IServiceCollection services)
    {
        // Register CRM Application services, command handlers, query handlers, validators, etc.
        return services;
    }
}
