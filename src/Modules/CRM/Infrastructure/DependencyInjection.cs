using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCRMInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register CRM Infrastructure services, DbContext, Repositories, etc.
        return services;
    }
}
