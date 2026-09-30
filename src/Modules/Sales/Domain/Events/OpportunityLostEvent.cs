using BuildingBlocks.Domain.Events;

namespace Modules.Sales.Domain.Events;

public record OpportunityLostEvent(
    Guid OpportunityId,
    string LossReason,
    DateTime LostAt
) : DomainEvent;
