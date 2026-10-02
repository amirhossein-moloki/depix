using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Domain.Repositories;

public interface IWorkTaskRepository
{
    Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<WorkTask>> GetListAsync(Guid? assignedTo = null, string? status = null, CancellationToken cancellationToken = default);
    Task AddAsync(WorkTask task, CancellationToken cancellationToken = default);
    void Update(WorkTask task);
}
