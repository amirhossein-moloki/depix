using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

public class OpportunityRepository : IOpportunityRepository
{
    private readonly DbContext _dbContext;

    public OpportunityRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Opportunity>()
            .Include(o => o.Proposals)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, cancellationToken);
    }

    public async Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Opportunity>().AddAsync(opportunity, cancellationToken);
    }

    public void Update(Opportunity opportunity)
    {
        _dbContext.Set<Opportunity>().Update(opportunity);
    }

    public async Task<(List<Opportunity> Items, int TotalCount)> GetFilteredAsync(
        OpportunityFilterParams filterParams,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Opportunity>().AsNoTracking().Where(o => !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var searchTerm = filterParams.Search.Trim().ToLower();
            query = query.Where(o =>
                o.Title.ToLower().Contains(searchTerm) ||
                (o.Description != null && o.Description.ToLower().Contains(searchTerm)) ||
                (o.Source != null && o.Source.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Stage))
        {
            query = query.Where(o => o.Stage == filterParams.Stage.Trim());
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Status))
        {
            query = query.Where(o => o.Status == filterParams.Status.Trim());
        }

        if (filterParams.CustomerId.HasValue && filterParams.CustomerId.Value != Guid.Empty)
        {
            query = query.Where(o => o.CustomerId == filterParams.CustomerId.Value);
        }

        if (filterParams.LeadId.HasValue && filterParams.LeadId.Value != Guid.Empty)
        {
            query = query.Where(o => o.LeadId == filterParams.LeadId.Value);
        }

        if (filterParams.CompanyId.HasValue && filterParams.CompanyId.Value != Guid.Empty)
        {
            query = query.Where(o => o.CompanyId == filterParams.CompanyId.Value);
        }

        if (filterParams.AssignedTo.HasValue && filterParams.AssignedTo.Value != Guid.Empty)
        {
            query = query.Where(o => o.AssignedTo == filterParams.AssignedTo.Value);
        }

        if (filterParams.MinValue.HasValue)
        {
            query = query.Where(o => o.Value.Amount >= filterParams.MinValue.Value);
        }

        if (filterParams.MaxValue.HasValue)
        {
            query = query.Where(o => o.Value.Amount <= filterParams.MaxValue.Value);
        }

        if (filterParams.FromCloseDate.HasValue)
        {
            query = query.Where(o => o.ExpectedCloseDate >= filterParams.FromCloseDate.Value);
        }

        if (filterParams.ToCloseDate.HasValue)
        {
            query = query.Where(o => o.ExpectedCloseDate <= filterParams.ToCloseDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = (filterParams.SortBy?.ToLower()) switch
        {
            "title" => filterParams.SortDescending ? query.OrderByDescending(o => o.Title) : query.OrderBy(o => o.Title),
            "stage" => filterParams.SortDescending ? query.OrderByDescending(o => o.Stage) : query.OrderBy(o => o.Stage),
            "value" => filterParams.SortDescending ? query.OrderByDescending(o => o.Value.Amount) : query.OrderBy(o => o.Value.Amount),
            "expectedclosedate" => filterParams.SortDescending ? query.OrderByDescending(o => o.ExpectedCloseDate) : query.OrderBy(o => o.ExpectedCloseDate),
            _ => filterParams.SortDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
        };

        var page = filterParams.Page < 1 ? 1 : filterParams.Page;
        var pageSize = filterParams.PageSize < 1 ? 10 : filterParams.PageSize;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<OpportunityPipelineStageSummary>> GetPipelineSummaryAsync(
        Guid? companyId = null,
        Guid? customerId = null,
        Guid? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Opportunity>().AsNoTracking().Where(o => !o.IsDeleted);

        if (companyId.HasValue && companyId.Value != Guid.Empty)
        {
            query = query.Where(o => o.CompanyId == companyId.Value);
        }

        if (customerId.HasValue && customerId.Value != Guid.Empty)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        if (assignedTo.HasValue && assignedTo.Value != Guid.Empty)
        {
            query = query.Where(o => o.AssignedTo == assignedTo.Value);
        }

        var allOpportunities = await query.ToListAsync(cancellationToken);

        var pipelineSummaries = new List<OpportunityPipelineStageSummary>();

        foreach (var stage in OpportunityStage.AllStages)
        {
            var stageItems = allOpportunities
                .Where(o => o.Stage.Equals(stage, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var totalValue = stageItems.Sum(o => o.Value?.Amount ?? 0m);

            pipelineSummaries.Add(new OpportunityPipelineStageSummary(
                stage,
                stageItems.Count,
                totalValue,
                stageItems
            ));
        }

        return pipelineSummaries;
    }
}
