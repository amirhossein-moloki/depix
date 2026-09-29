using BuildingBlocks.Domain.Events;

namespace Modules.CRM.Domain.Events;

public record LeadQualifiedEvent(Guid LeadId, Guid CompanyId) : DomainEvent;
