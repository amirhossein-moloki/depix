using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Reporting.Application;

namespace Modules.Reporting.API;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReportingApplication(configuration);
        return services;
    }
}
