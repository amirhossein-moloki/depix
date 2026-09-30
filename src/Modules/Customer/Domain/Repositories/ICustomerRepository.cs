using CustomerEntity = Modules.Customer.Domain.Entities.Customer;

namespace Modules.Customer.Domain.Repositories;

public interface ICustomerRepository
{
    Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CustomerEntity?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<CustomerEntity?> GetByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken = default);
    Task<bool> ExistsForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<(List<CustomerEntity> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? status = null,
        Guid? companyId = null,
        Guid? assignedTo = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);
    Task AddAsync(CustomerEntity customer, CancellationToken cancellationToken = default);
    void Update(CustomerEntity customer);
}
