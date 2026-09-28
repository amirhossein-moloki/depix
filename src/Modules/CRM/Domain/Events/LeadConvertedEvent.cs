using BuildingBlocks.Domain.Events;

namespace Modules.CRM.Domain.Events;

public record LeadConvertedEvent(Guid LeadId, Guid CompanyId) : DomainEvent;
