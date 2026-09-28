using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Finance.Domain.Repositories;
using Modules.Finance.Infrastructure.Persistence.Repositories;

namespace Modules.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
        return services;
    }
}
