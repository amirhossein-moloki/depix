using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Infrastructure.Persistence.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly DbContext _dbContext;

    public CompanyRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Company>()
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Company>> GetListAsync(int page, int pageSize, string? search, string? type, string? industry, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<Company>().AsNoTracking(), search, type, industry);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(string? search, string? type, string? industry, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<Company>().AsNoTracking(), search, type, industry);
        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<Company> ApplyFilters(IQueryable<Company> query, string? search, string? type, string? industry)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(searchLower) || c.Email.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var typeLower = type.Trim().ToLower();
            query = query.Where(c => c.Type.ToLower() == typeLower);
        }

        if (!string.IsNullOrWhiteSpace(industry))
        {
            var industryLower = industry.Trim().ToLower();
            query = query.Where(c => c.Industry.ToLower() == industryLower);
        }

        return query;
    }

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Company>().AddAsync(company, cancellationToken);
    }

    public void Update(Company company)
    {
        _dbContext.Set<Company>().Update(company);
    }
}
