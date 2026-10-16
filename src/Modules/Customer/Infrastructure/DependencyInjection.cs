using BuildingBlocks.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Domain.Repositories;
using Modules.Customer.Infrastructure.Persistence.Repositories;
using Modules.Customer.Infrastructure.Services;

namespace Modules.Customer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerService, CustomerService>();
        return services;
    }
}
