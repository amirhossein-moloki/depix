using BuildingBlocks.Domain.Events;

namespace Modules.Customer.Domain.Events;

public record CustomerArchivedEvent(Guid CustomerId, Guid? ArchivedBy) : DomainEvent;
