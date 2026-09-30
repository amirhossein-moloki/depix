using BuildingBlocks.Domain.Events;

namespace Modules.Customer.Domain.Events;

public record CustomerReactivatedEvent(Guid CustomerId) : DomainEvent;
