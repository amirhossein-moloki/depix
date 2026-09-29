using BuildingBlocks.Domain.Events;

namespace Modules.CRM.Domain.Events;

public record LeadAssignedEvent(Guid LeadId, Guid? AssignedTo) : DomainEvent;
