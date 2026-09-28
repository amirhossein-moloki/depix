using Modules.Support.Domain.Entities;

namespace Modules.Support.Domain.Repositories;

public interface ISupportPlanRepository
{
    Task<SupportPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(SupportPlan plan, CancellationToken cancellationToken = default);
    void Update(SupportPlan plan);
}
