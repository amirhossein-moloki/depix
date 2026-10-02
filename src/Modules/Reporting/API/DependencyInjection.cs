using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Modules.Reporting.API;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingApi(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
