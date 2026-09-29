using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Infrastructure.Persistence.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly DbContext _dbContext;

    public ContactRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Contact>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Contact>> GetListAsync(int page, int pageSize, Guid? companyId, string? search, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<Contact>().AsNoTracking(), companyId, search);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(Guid? companyId, string? search, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<Contact>().AsNoTracking(), companyId, search);
        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<Contact> ApplyFilters(IQueryable<Contact> query, Guid? companyId, string? search)
    {
        if (companyId.HasValue && companyId.Value != Guid.Empty)
        {
            query = query.Where(c => c.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(searchLower) ||
                c.LastName.ToLower().Contains(searchLower) ||
                c.Name.ToLower().Contains(searchLower) ||
                c.Email.ToLower().Contains(searchLower) ||
                c.Phone.ToLower().Contains(searchLower));
        }

        return query;
    }

    public async Task AddAsync(Contact contact, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Contact>().AddAsync(contact, cancellationToken);
    }

    public void Update(Contact contact)
    {
        _dbContext.Set<Contact>().Update(contact);
    }
}
