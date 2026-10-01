using BuildingBlocks.Domain.Events;

namespace Modules.Customer.Domain.Events;

public record CustomerPrimaryContactChangedEvent(Guid CustomerId, Guid? PrimaryContactId) : DomainEvent;
