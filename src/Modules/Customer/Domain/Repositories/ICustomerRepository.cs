using CustomerEntity = Modules.Customer.Domain.Entities.Customer;

namespace Modules.Customer.Domain.Repositories;

public interface ICustomerRepository
{
    Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerEntity?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task AddAsync(CustomerEntity customer, CancellationToken cancellationToken = default);
    void Update(CustomerEntity customer);
}
