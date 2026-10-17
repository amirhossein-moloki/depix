using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Infrastructure.Persistence.Repositories;

public class SalesVisitRepository : ISalesVisitRepository
{
    private readonly DbContext _dbContext;

    public SalesVisitRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SalesVisit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SalesVisit>()
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, cancellationToken);
    }

    public async Task<List<SalesVisit>> GetByCompanyIdAsync(Guid companyId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SalesVisit>()
            .Where(v => v.CompanyId == companyId && !v.IsDeleted)
            .OrderByDescending(v => v.VisitDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SalesVisit>> GetVisitsAsync(
        Guid? companyId = null,
        Guid? salespersonId = null,
        string? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<SalesVisit>().Where(v => !v.IsDeleted);

        if (companyId.HasValue)
        {
            query = query.Where(v => v.CompanyId == companyId.Value);
        }

        if (salespersonId.HasValue)
        {
            query = query.Where(v => v.SalespersonId == salespersonId.Value);
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            var normalizedResult = result.Trim().ToUpperInvariant();
            query = query.Where(v => v.Result == normalizedResult);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(v => v.VisitDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(v => v.VisitDate <= toDate.Value);
        }

        return await query
            .OrderByDescending(v => v.VisitDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetVisitsCountAsync(
        Guid? companyId = null,
        Guid? salespersonId = null,
        string? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<SalesVisit>().Where(v => !v.IsDeleted);

        if (companyId.HasValue)
        {
            query = query.Where(v => v.CompanyId == companyId.Value);
        }

        if (salespersonId.HasValue)
        {
            query = query.Where(v => v.SalespersonId == salespersonId.Value);
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            var normalizedResult = result.Trim().ToUpperInvariant();
            query = query.Where(v => v.Result == normalizedResult);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(v => v.VisitDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(v => v.VisitDate <= toDate.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(SalesVisit visit, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<SalesVisit>().AddAsync(visit, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SalesVisit visit, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<SalesVisit>().Update(visit);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
