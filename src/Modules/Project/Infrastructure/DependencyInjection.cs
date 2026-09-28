using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Project.Domain.Repositories;
using Modules.Project.Infrastructure.Persistence.Repositories;

namespace Modules.Project.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProjectRepository, ProjectRepository>();
        return services;
    }
}
