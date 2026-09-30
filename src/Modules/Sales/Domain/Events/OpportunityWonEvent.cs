using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.ValueObjects;

namespace Modules.Sales.Domain.Events;

public record OpportunityWonEvent(
    Guid OpportunityId,
    Money Value,
    DateTime WonAt
) : DomainEvent;
