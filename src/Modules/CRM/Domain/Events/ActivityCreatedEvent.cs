using BuildingBlocks.Domain.Events;

namespace Modules.CRM.Domain.Events;

public record ActivityCreatedEvent(Guid ActivityId, Guid LeadId, string Type, Guid? UserId) : DomainEvent;
