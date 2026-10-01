using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

public class ProposalRepository : IProposalRepository
{
    private readonly DbContext _dbContext;

    public ProposalRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Proposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Proposal>()
            .Include(p => p.ProposalItems)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Proposal>> GetByOpportunityIdAsync(Guid opportunityId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Proposal>()
            .Include(p => p.ProposalItems)
            .Where(p => p.OpportunityId == opportunityId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<Proposal> Items, int TotalCount)> GetFilteredAsync(ProposalFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Proposal>()
            .Include(p => p.ProposalItems)
            .AsNoTracking()
            .AsQueryable();

        if (filterParams.OpportunityId.HasValue)
        {
            query = query.Where(p => p.OpportunityId == filterParams.OpportunityId.Value);
        }

        if (filterParams.CustomerId.HasValue)
        {
            query = query.Where(p => p.CustomerId == filterParams.CustomerId.Value);
        }

        if (filterParams.CompanyId.HasValue)
        {
            query = query.Where(p => p.CompanyId == filterParams.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Status))
        {
            var statusLower = filterParams.Status.Trim().ToLower();
            query = query.Where(p => p.Status.ToLower() == statusLower);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filterParams.Page - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Proposal>().AddAsync(proposal, cancellationToken);
    }

    public void Update(Proposal proposal)
    {
        _dbContext.Set<Proposal>().Update(proposal);
    }
}
