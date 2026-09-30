namespace Modules.Sales.Application.Features.Opportunities.DTOs;

public record OpportunityDto(
    Guid Id,
    string Title,
    string? Description,
    Guid? LeadId,
    Guid? CustomerId,
    Guid? CompanyId,
    Guid? ContactId,
    string Stage,
    string Status,
    decimal ValueAmount,
    string ValueCurrency,
    decimal EstimatedValue,
    int Probability,
    DateOnly ExpectedCloseDate,
    Guid? AssignedTo,
    string? Source,
    DateTime? WonAt,
    DateTime? LostAt,
    string? LossReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record OpportunityListItemDto(
    Guid Id,
    string Title,
    Guid? LeadId,
    Guid? CustomerId,
    Guid? CompanyId,
    Guid? ContactId,
    string Stage,
    string Status,
    decimal ValueAmount,
    string ValueCurrency,
    int Probability,
    DateOnly ExpectedCloseDate,
    Guid? AssignedTo,
    DateTime CreatedAt
);

public record PipelineStageGroupDto(
    string Stage,
    int Count,
    decimal TotalValue,
    List<OpportunityListItemDto> Items
);

public record OpportunityPipelineDto(
    List<PipelineStageGroupDto> Stages,
    int TotalCount,
    decimal TotalPipelineValue
);

public record CreateOpportunityRequest(
    string Title,
    string? Description = null,
    Guid? LeadId = null,
    Guid? CustomerId = null,
    Guid? CompanyId = null,
    Guid? ContactId = null,
    string? Stage = null,
    decimal? ValueAmount = null,
    string? ValueCurrency = null,
    int? Probability = null,
    DateOnly? ExpectedCloseDate = null,
    Guid? AssignedTo = null,
    string? Source = null
);

public record UpdateOpportunityRequest(
    string Title,
    string? Description = null,
    decimal ValueAmount = 0m,
    string ValueCurrency = "USD",
    int Probability = 10,
    DateOnly ExpectedCloseDate = default,
    string? Source = null,
    Guid? ContactId = null
);

public record AssignOpportunityRequest(
    Guid? AssignedTo
);

public record ChangeOpportunityStageRequest(
    string Stage,
    int? Probability = null
);

public record MarkOpportunityWonRequest(
    DateTime? WonAt = null
);

public record MarkOpportunityLostRequest(
    string LossReason,
    DateTime? LostAt = null
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
