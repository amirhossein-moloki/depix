using BuildingBlocks.Domain.Events;

namespace Modules.CRM.Domain.Events;

public record LeadStatusChangedEvent(Guid LeadId, string OldStatus, string NewStatus) : DomainEvent;
