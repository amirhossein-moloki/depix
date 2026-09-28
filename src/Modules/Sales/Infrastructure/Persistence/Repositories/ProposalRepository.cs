using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

public class ProposalRepository : IProposalRepository
{
    private readonly DbContext _dbContext;

    public ProposalRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Proposal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Proposal>()
            .Include(p => p.ProposalItems)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Proposal proposal, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Proposal>().AddAsync(proposal, cancellationToken);
    }

    public void Update(Proposal proposal)
    {
        _dbContext.Set<Proposal>().Update(proposal);
    }
}
