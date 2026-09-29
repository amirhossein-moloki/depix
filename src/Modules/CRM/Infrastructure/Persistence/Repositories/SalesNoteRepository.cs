using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Infrastructure.Persistence.Repositories;

public class SalesNoteRepository : ISalesNoteRepository
{
    private readonly DbContext _dbContext;

    public SalesNoteRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SalesNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SalesNote>()
            .FirstOrDefaultAsync(sn => sn.Id == id, cancellationToken);
    }

    public async Task<List<SalesNote>> GetListAsync(
        int page,
        int pageSize,
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? createdBy,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<SalesNote>().AsNoTracking(), leadId, contactId, companyId, createdBy, search);

        return await query
            .OrderByDescending(sn => sn.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? createdBy,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Set<SalesNote>().AsNoTracking(), leadId, contactId, companyId, createdBy, search);
        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<SalesNote> ApplyFilters(
        IQueryable<SalesNote> query,
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? createdBy,
        string? search)
    {
        if (leadId.HasValue && leadId.Value != Guid.Empty)
        {
            query = query.Where(sn => sn.LeadId == leadId.Value);
        }

        if (contactId.HasValue && contactId.Value != Guid.Empty)
        {
            query = query.Where(sn => sn.ContactId == contactId.Value);
        }

        if (companyId.HasValue && companyId.Value != Guid.Empty)
        {
            query = query.Where(sn => sn.CompanyId == companyId.Value);
        }

        if (createdBy.HasValue && createdBy.Value != Guid.Empty)
        {
            query = query.Where(sn => sn.CreatedBy == createdBy.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(sn => sn.Title.ToLower().Contains(searchLower) ||
                                      sn.NeedAnalysis.ToLower().Contains(searchLower) ||
                                      sn.Objections.ToLower().Contains(searchLower) ||
                                      sn.Strategy.ToLower().Contains(searchLower));
        }

        return query;
    }

    public async Task AddAsync(SalesNote salesNote, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<SalesNote>().AddAsync(salesNote, cancellationToken);
    }

    public void Update(SalesNote salesNote)
    {
        _dbContext.Set<SalesNote>().Update(salesNote);
    }
}
