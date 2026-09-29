using BuildingBlocks.Domain.Events;

namespace Modules.CRM.Domain.Events;

public record SalesNoteCreatedEvent(Guid SalesNoteId, Guid LeadId, Guid CreatedBy) : DomainEvent;
