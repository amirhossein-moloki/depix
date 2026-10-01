using Modules.Sales.Domain.Entities;

namespace Modules.Sales.Domain.Repositories;

public record ProposalFilterParams(
    int Page = 1,
    int PageSize = 10,
    Guid? OpportunityId = null,
    Guid? CustomerId = null,
    Guid? CompanyId = null,
    string? Status = null
);

public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Proposal>> GetByOpportunityIdAsync(Guid opportunityId, CancellationToken cancellationToken = default);
    Task<(List<Proposal> Items, int TotalCount)> GetFilteredAsync(ProposalFilterParams filterParams, CancellationToken cancellationToken = default);
    Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default);
    void Update(Proposal proposal);
}
