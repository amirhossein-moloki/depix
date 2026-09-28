using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Domain.Repositories;
using Modules.Customer.Infrastructure.Persistence.Repositories;

namespace Modules.Customer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        return services;
    }
}
