namespace Modules.CRM.Application.Features.Companies.DTOs;

public record AddressDto(string Text);

public record CompanyDto(
    Guid Id,
    string Name,
    string Industry,
    string Website,
    string Phone,
    string Email,
    AddressDto Address,
    string Type,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsDeleted,
    DateTime? DeletedAt
);

public record CompanyListDto(
    Guid Id,
    string Name,
    string Industry,
    string Website,
    string Phone,
    string Email,
    string Type,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt
);

public record CreateCompanyRequest(
    string Name,
    string Industry,
    string Website,
    string Phone,
    string Email,
    string AddressText,
    string Type = "LEAD",
    double? Latitude = null,
    double? Longitude = null
);

public record UpdateCompanyRequest(
    string Name,
    string Industry,
    string Website,
    string Phone,
    string Email,
    string AddressText,
    string Type,
    double? Latitude = null,
    double? Longitude = null
);

public record UpdateCompanyLocationRequest(
    double? Latitude,
    double? Longitude
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
