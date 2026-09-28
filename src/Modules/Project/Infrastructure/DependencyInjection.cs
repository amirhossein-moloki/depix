using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Project.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Project Infrastructure services, DbContext, Repositories, etc.
        return services;
    }
}
