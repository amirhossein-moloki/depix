using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Application;

namespace Modules.Identity.API;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityApplication();
        return services;
    }
}
