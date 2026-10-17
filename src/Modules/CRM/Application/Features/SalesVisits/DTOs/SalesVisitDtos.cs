namespace Modules.CRM.Application.Features.SalesVisits.DTOs;

public record SalesVisitDto(
    Guid Id,
    Guid CompanyId,
    Guid SalespersonId,
    DateTime VisitedAt,
    string ReportText,
    string Result,
    DateTime? FollowUpAt,
    double? Latitude,
    double? Longitude,
    string? LocationSource,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record SalesVisitListDto(
    Guid Id,
    Guid CompanyId,
    Guid SalespersonId,
    DateTime VisitedAt,
    string Result,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt
);

public record CreateSalesVisitRequest(
    Guid CompanyId,
    DateTime VisitedAt,
    string ReportText,
    string Result,
    DateTime? FollowUpAt = null,
    double? Latitude = null,
    double? Longitude = null,
    string? LocationSource = null
);
