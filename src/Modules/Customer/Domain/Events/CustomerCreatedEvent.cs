using BuildingBlocks.Domain.Events;

namespace Modules.Customer.Domain.Events;

public record CustomerCreatedEvent(Guid CustomerId, Guid CompanyId, string CustomerNumber) : DomainEvent;
