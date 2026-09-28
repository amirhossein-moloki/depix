using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Domain.Repositories;
using Modules.Identity.Infrastructure.Authorization;
using Modules.Identity.Infrastructure.Persistence.Repositories;
using Modules.Identity.Infrastructure.Services;

namespace Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IdentityRepositories>();
        services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<IdentityRepositories>());
        services.AddScoped<IRoleRepository>(sp => sp.GetRequiredService<IdentityRepositories>());
        services.AddScoped<IPermissionRepository>(sp => sp.GetRequiredService<IdentityRepositories>());
        services.AddScoped<IRefreshTokenRepository>(sp => sp.GetRequiredService<IdentityRepositories>());

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IAuditEventLogger, AuditEventLogger>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
