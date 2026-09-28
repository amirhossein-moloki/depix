using Microsoft.EntityFrameworkCore;
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
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Opportunity>().AddAsync(opportunity, cancellationToken);
    }

    public void Update(Opportunity opportunity)
    {
        _dbContext.Set<Opportunity>().Update(opportunity);
    }
}
