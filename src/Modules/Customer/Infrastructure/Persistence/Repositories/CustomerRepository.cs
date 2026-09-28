using Microsoft.EntityFrameworkCore;
using CustomerEntity = Modules.Customer.Domain.Entities.Customer;
using Modules.Customer.Domain.Repositories;

namespace Modules.Customer.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly DbContext _dbContext;

    public CustomerRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<CustomerEntity>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<CustomerEntity?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<CustomerEntity>()
            .FirstOrDefaultAsync(c => c.CompanyId == companyId, cancellationToken);
    }

    public async Task AddAsync(CustomerEntity customer, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<CustomerEntity>().AddAsync(customer, cancellationToken);
    }

    public void Update(CustomerEntity customer)
    {
        _dbContext.Set<CustomerEntity>().Update(customer);
    }
}
