using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.ValueObjects;

namespace Modules.Sales.Domain.Events;

public record OpportunityCreatedEvent(
    Guid OpportunityId,
    string Title,
    Guid? LeadId,
    Guid? CustomerId,
    Guid? CompanyId,
    Money Value,
    string Stage
) : DomainEvent;
