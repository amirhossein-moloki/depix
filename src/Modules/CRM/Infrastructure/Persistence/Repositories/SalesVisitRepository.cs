using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Infrastructure.Persistence.Repositories;

public class SalesVisitRepository : ISalesVisitRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SalesVisitRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SalesVisit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SalesVisit>()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<List<SalesVisit>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SalesVisit>()
            .Where(v => v.CompanyId == companyId)
            .OrderByDescending(v => v.VisitedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<SalesVisit> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? companyId = null,
        Guid? salespersonId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? resultFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<SalesVisit>().AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(v => v.CompanyId == companyId.Value);
        }

        if (salespersonId.HasValue)
        {
            query = query.Where(v => v.SalespersonId == salespersonId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(v => v.VisitedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(v => v.VisitedAt <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(resultFilter))
        {
            var filter = resultFilter.Trim().ToLower();
            query = query.Where(v => v.Result.ToLower().Contains(filter));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.VisitedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(SalesVisit visit, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<SalesVisit>().AddAsync(visit, cancellationToken);
    }

    public void Update(SalesVisit visit)
    {
        _dbContext.Set<SalesVisit>().Update(visit);
    }
}
