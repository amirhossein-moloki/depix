namespace Modules.CRM.Application.DTOs;

public record CreateSalesVisitRequest(
    Guid CompanyId,
    string Report,
    string Result, // SUCCESSFUL, UNSUCCESSFUL, FOLLOW_UP_REQUIRED, PENDING
    Guid? CustomerId = null,
    DateTime? VisitDate = null,
    DateTime? FollowUpAt = null,
    double? Latitude = null,
    double? Longitude = null,
    string? LocationSource = null
);

public record SalesVisitDto(
    Guid Id,
    Guid CompanyId,
    Guid? CustomerId,
    Guid SalespersonId,
    DateTime VisitDate,
    string Report,
    string Result,
    DateTime? FollowUpAt,
    double? Latitude,
    double? Longitude,
    string? LocationSource,
    DateTime CreatedAt
);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
