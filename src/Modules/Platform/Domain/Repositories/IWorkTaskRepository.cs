using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Domain.Repositories;

public interface IWorkTaskRepository
{
    Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(WorkTask task, CancellationToken cancellationToken = default);
    void Update(WorkTask task);
}
