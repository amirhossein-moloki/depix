using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.Contacts.DTOs;

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
    DateTime? DeletedAt,
    CompanyDto? Company = null,
    ContactDto? Contact = null
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
    DateTime CreatedAt,
    CompanyDto? Company = null,
    ContactDto? Contact = null
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

public record CompositeCompanyDetails(
    string Name,
    string? Industry = null,
    string? Website = null,
    string? Phone = null,
    string? Email = null,
    string? AddressText = null,
    string Type = "LEAD"
);

public record CompositeContactDetails(
    string FirstName,
    string LastName,
    string? Email = null,
    string? Phone = null,
    string? Position = null,
    string? Description = null,
    bool IsDecisionMaker = false,
    string? InfluenceLevel = null
);

public record CreateLeadRequest(
    Guid? CompanyId = null,
    string Title = "",
    string Source = "",
    string? Description = null,
    decimal? EstimatedValue = null,
    Guid? ContactId = null,
    Guid? AssignedTo = null,
    int Score = 0,
    string Status = "NEW",
    CompositeCompanyDetails? Company = null,
    CompositeContactDetails? Contact = null
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
