using Modules.Sales.Domain.Entities;

namespace Modules.Sales.Domain.Repositories;

public interface IOpportunityRepository
{
    Task<Opportunity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
    void Update(Opportunity opportunity);
}
