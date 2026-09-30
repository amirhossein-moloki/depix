using BuildingBlocks.Domain.Events;

namespace Modules.Customer.Domain.Events;

public record CustomerAssignedEvent(Guid CustomerId, Guid? AssignedTo) : DomainEvent;
