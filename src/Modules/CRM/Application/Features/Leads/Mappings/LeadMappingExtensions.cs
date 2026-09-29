using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Application.Features.Leads.Mappings;

public static class LeadMappingExtensions
{
    public static LeadDto ToDto(this Lead lead)
    {
        return new LeadDto(
            lead.Id,
            lead.CompanyId,
            lead.ContactId,
            lead.Title,
            lead.Source,
            lead.Description,
            lead.Status,
            lead.Score,
            lead.EstimatedValue,
            lead.DisqualificationReason,
            lead.AssignedTo,
            lead.CreatedAt,
            lead.UpdatedAt,
            lead.IsDeleted,
            lead.DeletedAt
        );
    }

    public static LeadListItemDto ToListItemDto(this Lead lead)
    {
        return new LeadListItemDto(
            lead.Id,
            lead.CompanyId,
            lead.ContactId,
            lead.Title,
            lead.Source,
            lead.Status,
            lead.Score,
            lead.EstimatedValue,
            lead.AssignedTo,
            lead.CreatedAt
        );
    }
}
