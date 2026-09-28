using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Support.Domain.Repositories;
using Modules.Support.Infrastructure.Persistence.Repositories;

namespace Modules.Support.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSupportInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISupportPlanRepository, SupportPlanRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        return services;
    }
}
