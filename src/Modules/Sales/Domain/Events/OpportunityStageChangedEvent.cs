using BuildingBlocks.Domain.Events;

namespace Modules.Sales.Domain.Events;

public record OpportunityStageChangedEvent(
    Guid OpportunityId,
    string OldStage,
    string NewStage,
    int Probability
) : DomainEvent;
