using Microsoft.Extensions.DependencyInjection;

namespace Modules.Project.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectApplication(this IServiceCollection services)
    {
        // Register Project Application services, command handlers, query handlers, validators, etc.
        return services;
    }
}
