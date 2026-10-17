using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Application.Services;

namespace Modules.Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthUseCaseService, AuthUseCaseService>();
        services.AddScoped<IUserManagementUseCaseService, UserManagementService>();
        return services;
    }
}
