using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Infrastructure.Persistence.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly DbContext _dbContext;

    public LeadRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Lead>()
            .Include(l => l.Research)
            .Include(l => l.Activities)
            .Include(l => l.SalesNotes)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<List<Lead>> GetListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        Guid? companyId,
        Guid? assignedTo,
        string? source,
        DateTime? fromDate,
        DateTime? toDate,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<Lead>().AsNoTracking(), search, status, companyId, assignedTo, source, fromDate, toDate);

        query = ApplySorting(query, sortBy, sortDescending);

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? search,
        string? status,
        Guid? companyId,
        Guid? assignedTo,
        string? source,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<Lead>().AsNoTracking(), search, status, companyId, assignedTo, source, fromDate, toDate);
        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<Lead> ApplyFilters(
        IQueryable<Lead> query,
        string? search,
        string? status,
        Guid? companyId,
        Guid? assignedTo,
        string? source,
        DateTime? fromDate,
        DateTime? toDate)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(l => l.Title.ToLower().Contains(searchLower) ||
                                     l.Source.ToLower().Contains(searchLower) ||
                                     l.Description.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusLower = status.Trim().ToLower();
            query = query.Where(l => l.Status.ToLower() == statusLower);
        }

        if (companyId.HasValue && companyId.Value != Guid.Empty)
        {
            query = query.Where(l => l.CompanyId == companyId.Value);
        }

        if (assignedTo.HasValue && assignedTo.Value != Guid.Empty)
        {
            query = query.Where(l => l.AssignedTo == assignedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(source))
        {
            var sourceLower = source.Trim().ToLower();
            query = query.Where(l => l.Source.ToLower() == sourceLower);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= toDate.Value);
        }

        return query;
    }

    private static IQueryable<Lead> ApplySorting(IQueryable<Lead> query, string? sortBy, bool sortDescending)
    {
        var key = sortBy?.Trim().ToLower();
        return key switch
        {
            "title" => sortDescending ? query.OrderByDescending(l => l.Title) : query.OrderBy(l => l.Title),
            "status" => sortDescending ? query.OrderByDescending(l => l.Status) : query.OrderBy(l => l.Status),
            "score" => sortDescending ? query.OrderByDescending(l => l.Score) : query.OrderBy(l => l.Score),
            "estimatedvalue" => sortDescending ? query.OrderByDescending(l => l.EstimatedValue) : query.OrderBy(l => l.EstimatedValue),
            "source" => sortDescending ? query.OrderByDescending(l => l.Source) : query.OrderBy(l => l.Source),
            _ => sortDescending ? query.OrderByDescending(l => l.CreatedAt) : query.OrderByDescending(l => l.CreatedAt)
        };
    }

    public async Task AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Lead>().AddAsync(lead, cancellationToken);
    }

    public void Update(Lead lead)
    {
        _dbContext.Set<Lead>().Update(lead);
    }
}
