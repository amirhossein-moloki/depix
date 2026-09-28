using Microsoft.EntityFrameworkCore;
using Modules.Support.Domain.Entities;
using Modules.Support.Domain.Repositories;

namespace Modules.Support.Infrastructure.Persistence.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly DbContext _dbContext;

    public TicketRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Ticket>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Ticket>().AddAsync(ticket, cancellationToken);
    }

    public void Update(Ticket ticket)
    {
        _dbContext.Set<Ticket>().Update(ticket);
    }
}
