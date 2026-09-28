using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Platform.Domain.Repositories;
using Modules.Platform.Infrastructure.Persistence.Repositories;

namespace Modules.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFileAssetRepository, FileAssetRepository>();
        services.AddScoped<IWorkTaskRepository, WorkTaskRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        return services;
    }
}
