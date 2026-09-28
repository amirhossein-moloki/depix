using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Project.Application;

namespace Modules.Project.API;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProjectApplication();
        // Register Project API endpoints, controllers, request validators
        return services;
    }
}
