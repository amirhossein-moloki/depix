using BuildingBlocks.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Infrastructure.Persistence.Repositories;
using Modules.CRM.Infrastructure.Services;

namespace Modules.CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCRMInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<ISalesNoteRepository, SalesNoteRepository>();
        services.AddScoped<ICustomerContactService, CustomerContactService>();
        return services;
    }
}
