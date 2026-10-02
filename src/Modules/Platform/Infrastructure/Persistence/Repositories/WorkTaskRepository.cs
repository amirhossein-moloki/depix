using Microsoft.EntityFrameworkCore;
using Modules.Platform.Domain.Entities;
using Modules.Platform.Domain.Repositories;

namespace Modules.Platform.Infrastructure.Persistence.Repositories;

public class WorkTaskRepository : IWorkTaskRepository
{
    private readonly DbContext _dbContext;

    public WorkTaskRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<WorkTask>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<WorkTask>> GetListAsync(Guid? assignedTo = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<WorkTask>().AsQueryable();

        if (assignedTo.HasValue)
        {
            query = query.Where(t => t.AssignedTo == assignedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status == status);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<WorkTask>().AddAsync(task, cancellationToken);
    }

    public void Update(WorkTask task)
    {
        _dbContext.Set<WorkTask>().Update(task);
    }
}
