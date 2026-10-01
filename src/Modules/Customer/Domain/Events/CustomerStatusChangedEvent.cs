using BuildingBlocks.Domain.Events;

namespace Modules.Customer.Domain.Events;

public record CustomerStatusChangedEvent(Guid CustomerId, string PreviousStatus, string NewStatus) : DomainEvent;
