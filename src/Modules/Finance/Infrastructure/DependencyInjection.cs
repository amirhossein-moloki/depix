using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Finance.Application.Services;
using Modules.Finance.Domain.Repositories;
using Modules.Finance.Infrastructure.Persistence.Repositories;
using Modules.Finance.Infrastructure.Services;

namespace Modules.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();

        return services;
    }
}
