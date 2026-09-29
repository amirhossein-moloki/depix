namespace Modules.CRM.Application.Features.Leads.DTOs;

public record LeadDto(
    Guid Id,
    Guid CompanyId,
    Guid? ContactId,
    string Title,
    string Source,
    string Description,
    string Status,
    int Score,
    decimal? EstimatedValue,
    string? DisqualificationReason,
    Guid? AssignedTo,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsDeleted,
    DateTime? DeletedAt
);

public record LeadListItemDto(
    Guid Id,
    Guid CompanyId,
    Guid? ContactId,
    string Title,
    string Source,
    string Status,
    int Score,
    decimal? EstimatedValue,
    Guid? AssignedTo,
    DateTime CreatedAt
);

public record LeadPipelineStageDto(
    string Status,
    int Count,
    decimal TotalEstimatedValue,
    IReadOnlyList<LeadListItemDto> Leads
);

public record LeadPipelineDto(
    IReadOnlyList<LeadPipelineStageDto> Stages,
    int TotalLeadsCount,
    decimal TotalPipelineValue
);

public record CreateLeadRequest(
    Guid CompanyId,
    string Title,
    string Source,
    string? Description = null,
    decimal? EstimatedValue = null,
    Guid? ContactId = null,
    Guid? AssignedTo = null,
    int Score = 0,
    string Status = "NEW"
);

public record UpdateLeadRequest(
    string Title,
    string Source,
    string? Description = null,
    decimal? EstimatedValue = null,
    Guid? ContactId = null
);

public record AssignLeadRequest(
    Guid? AssignedTo
);

public record ChangeLeadStatusRequest(
    string Status,
    string? Reason = null
);

public record QualifyLeadRequest();

public record DisqualifyLeadRequest(
    string Reason
);
