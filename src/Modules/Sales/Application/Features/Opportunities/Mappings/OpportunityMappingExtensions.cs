using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Domain.Entities;

namespace Modules.Sales.Application.Features.Opportunities.Mappings;

public static class OpportunityMappingExtensions
{
    public static OpportunityDto ToDto(this Opportunity opportunity)
    {
        return new OpportunityDto(
            opportunity.Id,
            opportunity.Title,
            opportunity.Description,
            opportunity.LeadId,
            opportunity.CustomerId,
            opportunity.CompanyId,
            opportunity.ContactId,
            opportunity.Stage,
            opportunity.Status,
            opportunity.Value?.Amount ?? 0m,
            opportunity.Value?.Currency ?? "USD",
            opportunity.EstimatedValue,
            opportunity.Probability,
            opportunity.ExpectedCloseDate,
            opportunity.AssignedTo,
            opportunity.Source,
            opportunity.WonAt,
            opportunity.LostAt,
            opportunity.LossReason,
            opportunity.CreatedAt,
            opportunity.UpdatedAt
        );
    }

    public static OpportunityListItemDto ToListItemDto(this Opportunity opportunity)
    {
        return new OpportunityListItemDto(
            opportunity.Id,
            opportunity.Title,
            opportunity.LeadId,
            opportunity.CustomerId,
            opportunity.CompanyId,
            opportunity.ContactId,
            opportunity.Stage,
            opportunity.Status,
            opportunity.Value?.Amount ?? 0m,
            opportunity.Value?.Currency ?? "USD",
            opportunity.Probability,
            opportunity.ExpectedCloseDate,
            opportunity.AssignedTo,
            opportunity.CreatedAt
        );
    }
}
