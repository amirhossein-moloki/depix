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

    public async Task<CustomerEntity?> GetByCustomerNumberAsync(string customerNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerNumber))
            return null;

        var normalized = customerNumber.Trim().ToUpper();
        return await _dbContext.Set<CustomerEntity>()
            .FirstOrDefaultAsync(c => c.CustomerNumber.ToUpper() == normalized, cancellationToken);
    }

    public async Task<bool> ExistsForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<CustomerEntity>()
            .AnyAsync(c => c.CompanyId == companyId, cancellationToken);
    }

    public async Task<(List<CustomerEntity> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? status = null,
        Guid? companyId = null,
        Guid? assignedTo = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<CustomerEntity>().AsNoTracking();

        if (companyId.HasValue)
        {
            query = query.Where(c => c.CompanyId == companyId.Value);
        }

        if (assignedTo.HasValue)
        {
            query = query.Where(c => c.AssignedTo == assignedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToUpperInvariant();
            query = query.Where(c => c.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search.Trim().ToLower()}%";
            query = query.Where(c =>
                EF.Functions.Like(c.CustomerNumber.ToLower(), searchPattern) ||
                (c.Notes != null && EF.Functions.Like(c.Notes.ToLower(), searchPattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLowerInvariant() switch
        {
            "customernumber" => sortDescending ? query.OrderByDescending(c => c.CustomerNumber) : query.OrderBy(c => c.CustomerNumber),
            "status" => sortDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "customersince" => sortDescending ? query.OrderByDescending(c => c.CustomerSince) : query.OrderBy(c => c.CustomerSince),
            _ => sortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
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
