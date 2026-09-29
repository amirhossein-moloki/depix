using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Infrastructure.Persistence.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly DbContext _dbContext;

    public ActivityRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Activity>()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Activity>> GetListAsync(
        int page,
        int pageSize,
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? userId,
        string? type,
        string? result,
        DateTime? fromDate,
        DateTime? toDate,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<Activity>().AsNoTracking(), leadId, contactId, companyId, userId, type, result, fromDate, toDate, search);

        return await query
            .OrderByDescending(a => a.OccurredAt)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? userId,
        string? type,
        string? result,
        DateTime? fromDate,
        DateTime? toDate,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<Activity>().AsNoTracking(), leadId, contactId, companyId, userId, type, result, fromDate, toDate, search);
        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<Activity> ApplyFilters(
        IQueryable<Activity> query,
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? userId,
        string? type,
        string? result,
        DateTime? fromDate,
        DateTime? toDate,
        string? search)
    {
        if (leadId.HasValue && leadId.Value != Guid.Empty)
        {
            query = query.Where(a => a.LeadId == leadId.Value);
        }

        if (contactId.HasValue && contactId.Value != Guid.Empty)
        {
            query = query.Where(a => a.ContactId == contactId.Value);
        }

        if (companyId.HasValue && companyId.Value != Guid.Empty)
        {
            query = query.Where(a => a.CompanyId == companyId.Value);
        }

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var typeLower = type.Trim().ToLower();
            query = query.Where(a => a.Type.ToLower() == typeLower);
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            var resultLower = result.Trim().ToLower();
            query = query.Where(a => a.Result.ToLower().Contains(resultLower));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.OccurredAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.OccurredAt <= toDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(a => a.Subject.ToLower().Contains(searchLower) ||
                                     a.Description.ToLower().Contains(searchLower) ||
                                     a.Result.ToLower().Contains(searchLower));
        }

        return query;
    }

    public async Task AddAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Activity>().AddAsync(activity, cancellationToken);
    }

    public void Update(Activity activity)
    {
        _dbContext.Set<Activity>().Update(activity);
    }
}
