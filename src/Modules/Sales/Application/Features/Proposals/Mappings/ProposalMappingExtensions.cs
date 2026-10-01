using Modules.Sales.Application.Features.Proposals.DTOs;
using Modules.Sales.Domain.Entities;

namespace Modules.Sales.Application.Features.Proposals.Mappings;

public static class ProposalMappingExtensions
{
    public static ProposalDto ToDto(this Proposal proposal)
    {
        return new ProposalDto(
            proposal.Id,
            proposal.OpportunityId,
            proposal.CustomerId,
            proposal.CompanyId,
            proposal.Title,
            proposal.Version,
            proposal.Description,
            proposal.Notes,
            proposal.ValidUntil,
            proposal.Status,
            proposal.Subtotal?.Amount ?? 0m,
            proposal.Discount?.Amount ?? proposal.DiscountAmount,
            proposal.Total?.Amount ?? proposal.Amount,
            proposal.Currency ?? "USD",
            proposal.ProposalItems?.Select(i => i.ToDto(proposal.Currency ?? "USD")).ToList() ?? new List<ProposalItemDto>(),
            proposal.CreatedAt,
            proposal.UpdatedAt
        );
    }

    public static ProposalItemDto ToDto(this ProposalItem item, string currency = "USD")
    {
        return new ProposalItemDto(
            item.Id,
            item.ProposalId,
            item.Name,
            item.Description,
            item.Quantity,
            item.UnitPrice?.Amount ?? item.Price,
            item.Discount?.Amount ?? 0m,
            item.Total?.Amount ?? (item.Price * item.Quantity),
            item.UnitPrice?.Currency ?? currency
        );
    }

    public static ProposalListItemDto ToListItemDto(this Proposal proposal)
    {
        return new ProposalListItemDto(
            proposal.Id,
            proposal.OpportunityId,
            proposal.CustomerId,
            proposal.CompanyId,
            proposal.Title,
            proposal.Version,
            proposal.ValidUntil,
            proposal.Status,
            proposal.Total?.Amount ?? proposal.Amount,
            proposal.Currency ?? "USD",
            proposal.ProposalItems?.Count ?? 0,
            proposal.CreatedAt
        );
    }
}
