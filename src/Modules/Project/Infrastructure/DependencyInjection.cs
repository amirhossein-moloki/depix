using BuildingBlocks.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Project.Domain.Repositories;
using Modules.Project.Infrastructure.Persistence.Repositories;
using Modules.Project.Infrastructure.Services;

namespace Modules.Project.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectService, ProjectService>();
        return services;
    }
}
