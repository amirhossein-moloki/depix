using Modules.Sales.Domain.Entities;

namespace Modules.Sales.Domain.Repositories;

public record OpportunityFilterParams(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Stage = null,
    string? Status = null,
    Guid? CustomerId = null,
    Guid? LeadId = null,
    Guid? CompanyId = null,
    Guid? AssignedTo = null,
    decimal? MinValue = null,
    decimal? MaxValue = null,
    DateOnly? FromCloseDate = null,
    DateOnly? ToCloseDate = null,
    string? SortBy = null,
    bool SortDescending = true
);

public record OpportunityPipelineStageSummary(
    string Stage,
    int Count,
    decimal TotalValue,
    List<Opportunity> Items
);

public interface IOpportunityRepository
{
    Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
    void Update(Opportunity opportunity);
    Task<(List<Opportunity> Items, int TotalCount)> GetFilteredAsync(OpportunityFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<List<OpportunityPipelineStageSummary>> GetPipelineSummaryAsync(
        Guid? companyId = null,
        Guid? customerId = null,
        Guid? assignedTo = null,
        CancellationToken cancellationToken = default);
}
