using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Domain.Repositories;
using Modules.Sales.Infrastructure.Persistence.Repositories;

namespace Modules.Sales.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IProposalRepository, ProposalRepository>();
        return services;
    }
}
