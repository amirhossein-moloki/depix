using BuildingBlocks.Application.Contracts;
using Modules.Customer.Domain.Repositories;
using CustomerEntity = Modules.Customer.Domain.Entities.Customer;

namespace Modules.Customer.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Guid> GetOrCreateCustomerForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var existingCustomer = await _customerRepository.GetByCompanyIdAsync(companyId, cancellationToken);
        if (existingCustomer != null)
        {
            return existingCustomer.Id;
        }

        var year = DateTime.UtcNow.Year;
        var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
        var customerNumber = $"CUST-{year}-{randomPart}";

        var newCustomer = CustomerEntity.Create(companyId, customerNumber, DateOnly.FromDateTime(DateTime.UtcNow));
        await _customerRepository.AddAsync(newCustomer, cancellationToken);
        return newCustomer.Id;
    }

    public async Task<bool> CustomerExistsForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _customerRepository.ExistsForCompanyAsync(companyId, cancellationToken);
    }

    public async Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        return customer != null && !customer.IsDeleted && customer.Status != "ARCHIVED";
    }
}
