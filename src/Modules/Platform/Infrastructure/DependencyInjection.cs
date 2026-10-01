using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Platform.Domain.Repositories;
using Modules.Platform.Domain.Services;
using Modules.Platform.Infrastructure.Persistence.Repositories;
using Modules.Platform.Infrastructure.Storage;

namespace Modules.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddScoped<IFileAssetRepository, FileAssetRepository>();
        services.AddScoped<IWorkTaskRepository, WorkTaskRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();

        return services;
    }
}
