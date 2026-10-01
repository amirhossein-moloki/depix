using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.ValueObjects;

namespace Modules.Sales.Domain.Events;

public record ProposalCreatedEvent(
    Guid ProposalId,
    Guid OpportunityId,
    string Title,
    Money TotalAmount
) : DomainEvent;

public record ProposalAcceptedEvent(
    Guid ProposalId,
    Guid OpportunityId,
    DateTime AcceptedAt
) : DomainEvent;

public record ProposalRejectedEvent(
    Guid ProposalId,
    Guid OpportunityId,
    string? Reason,
    DateTime RejectedAt
) : DomainEvent;
