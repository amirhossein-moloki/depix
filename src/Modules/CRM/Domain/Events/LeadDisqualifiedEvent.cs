using BuildingBlocks.Domain.Events;

namespace Modules.CRM.Domain.Events;

public record LeadDisqualifiedEvent(Guid LeadId, Guid CompanyId, string Reason) : DomainEvent;
