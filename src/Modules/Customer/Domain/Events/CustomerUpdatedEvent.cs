using BuildingBlocks.Domain.Events;

namespace Modules.Customer.Domain.Events;

public record CustomerUpdatedEvent(Guid CustomerId) : DomainEvent;
