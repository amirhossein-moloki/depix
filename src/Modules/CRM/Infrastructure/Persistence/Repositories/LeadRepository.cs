using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Infrastructure.Persistence.Repositories;

public class LeadRepository : ILeadRepository
{
    private readonly DbContext _dbContext;

    public LeadRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Lead>()
            .Include(l => l.Research)
            .Include(l => l.Activities)
            .Include(l => l.SalesNotes)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task AddAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Lead>().AddAsync(lead, cancellationToken);
    }

    public void Update(Lead lead)
    {
        _dbContext.Set<Lead>().Update(lead);
    }
}
