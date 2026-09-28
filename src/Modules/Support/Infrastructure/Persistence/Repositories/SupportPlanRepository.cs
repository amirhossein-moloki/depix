using Microsoft.EntityFrameworkCore;
using Modules.Support.Domain.Entities;
using Modules.Support.Domain.Repositories;

namespace Modules.Support.Infrastructure.Persistence.Repositories;

public class SupportPlanRepository : ISupportPlanRepository
{
    private readonly DbContext _dbContext;

    public SupportPlanRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SupportPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<SupportPlan>()
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
    }

    public async Task AddAsync(SupportPlan plan, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<SupportPlan>().AddAsync(plan, cancellationToken);
    }

    public void Update(SupportPlan plan)
    {
        _dbContext.Set<SupportPlan>().Update(plan);
    }
}
