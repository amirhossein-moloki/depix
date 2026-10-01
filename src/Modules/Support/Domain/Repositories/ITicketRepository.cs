using Modules.Support.Domain.Entities;

namespace Modules.Support.Domain.Repositories;

public record TicketFilterParams(
    Guid? CustomerId = null,
    Guid? ProjectId = null,
    Guid? ContactId = null,
    string? Status = null,
    string? Priority = null,
    string? Category = null,
    Guid? AssignedToUserId = null,
    string? SearchTerm = null,
    bool? UnresolvedOnly = null,
    DateTime? OpenedFrom = null,
    DateTime? OpenedTo = null,
    int PageNumber = 1,
    int PageSize = 20
);

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    Task<List<Ticket>> GetTicketsAsync(TicketFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(TicketFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<List<Ticket>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<Ticket>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
    void Update(Ticket ticket);
    void Delete(Ticket ticket);
}
