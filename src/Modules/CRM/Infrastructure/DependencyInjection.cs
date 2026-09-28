using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Infrastructure.Persistence.Repositories;

namespace Modules.CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCRMInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        return services;
    }
}
