namespace Modules.Sales.Application.Features.Proposals.DTOs;

public record ProposalItemDto(
    Guid Id,
    Guid ProposalId,
    string Name,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal Total,
    string Currency
);

public record ProposalDto(
    Guid Id,
    Guid OpportunityId,
    Guid? CustomerId,
    Guid? CompanyId,
    string Title,
    string Version,
    string Description,
    string Notes,
    DateOnly ValidUntil,
    string Status,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    string Currency,
    List<ProposalItemDto> ProposalItems,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ProposalListItemDto(
    Guid Id,
    Guid OpportunityId,
    Guid? CustomerId,
    Guid? CompanyId,
    string Title,
    string Version,
    DateOnly ValidUntil,
    string Status,
    decimal Total,
    string Currency,
    int ItemCount,
    DateTime CreatedAt
);

public record CreateProposalRequest(
    Guid OpportunityId,
    string Title,
    DateOnly ValidUntil,
    string Version = "1.0",
    string? Description = null,
    Guid? CustomerId = null,
    Guid? CompanyId = null,
    string? Notes = null,
    string Currency = "USD"
);

public record UpdateProposalRequest(
    string Title,
    string Description,
    DateOnly ValidUntil,
    string Version,
    string? Notes = null
);

public record AddProposalItemRequest(
    string Name,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount = 0m
);

public record UpdateProposalItemRequest(
    string Name,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount = 0m
);

public record ChangeProposalStatusRequest(
    string Status,
    string? Reason = null
);
