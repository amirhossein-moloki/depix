using Microsoft.EntityFrameworkCore;
using Modules.Support.Domain.Constants;
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
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Ticket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Ticket>()
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, cancellationToken);
    }

    public async Task<List<Ticket>> GetTicketsAsync(TicketFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filterParams);

        var pageNumber = filterParams.PageNumber < 1 ? 1 : filterParams.PageNumber;
        var pageSize = filterParams.PageSize < 1 ? 20 : filterParams.PageSize;

        return await query
            .Include(t => t.Comments)
            .OrderByDescending(t => t.OpenedAt)
            .ThenBy(t => t.TicketNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(TicketFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filterParams);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<List<Ticket>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Ticket>()
            .Include(t => t.Comments)
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Ticket>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Ticket>()
            .Include(t => t.Comments)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Ticket>().AddAsync(ticket, cancellationToken);
    }

    public void Update(Ticket ticket)
    {
        var entry = _dbContext.Entry(ticket);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Set<Ticket>().Update(ticket);
        }
        else
        {
            foreach (var comment in ticket.Comments)
            {
                var commentEntry = _dbContext.Entry(comment);
                if (commentEntry.State == EntityState.Detached)
                {
                    _dbContext.Set<TicketComment>().Add(comment);
                }
            }
        }
    }

    public void Delete(Ticket ticket)
    {
        _dbContext.Set<Ticket>().Remove(ticket);
    }

    private IQueryable<Ticket> BuildQuery(TicketFilterParams filterParams)
    {
        var query = _dbContext.Set<Ticket>().AsQueryable();

        if (filterParams.CustomerId.HasValue)
        {
            query = query.Where(t => t.CustomerId == filterParams.CustomerId.Value);
        }

        if (filterParams.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == filterParams.ProjectId.Value);
        }

        if (filterParams.ContactId.HasValue)
        {
            query = query.Where(t => t.ContactId == filterParams.ContactId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Status))
        {
            query = query.Where(t => t.Status == filterParams.Status.ToUpper());
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Priority))
        {
            query = query.Where(t => t.Priority == filterParams.Priority.ToUpper());
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Category))
        {
            query = query.Where(t => t.Category == filterParams.Category.ToUpper());
        }

        if (filterParams.AssignedToUserId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == filterParams.AssignedToUserId.Value);
        }

        if (filterParams.UnresolvedOnly.HasValue && filterParams.UnresolvedOnly.Value)
        {
            query = query.Where(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Cancelled);
        }

        if (filterParams.OpenedFrom.HasValue)
        {
            query = query.Where(t => t.OpenedAt >= filterParams.OpenedFrom.Value);
        }

        if (filterParams.OpenedTo.HasValue)
        {
            query = query.Where(t => t.OpenedAt <= filterParams.OpenedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var term = filterParams.SearchTerm.Trim().ToLower();
            query = query.Where(t =>
                t.TicketNumber.ToLower().Contains(term) ||
                t.Subject.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term));
        }

        return query;
    }
}
