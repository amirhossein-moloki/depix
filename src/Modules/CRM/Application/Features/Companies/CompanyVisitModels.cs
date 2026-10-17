namespace Modules.CRM.Application.Features.Companies;

public record CreateCompanyVisitRequest(
    Guid CompanyId,
    DateTime VisitedAt,
    string Notes,
    string? Outcome = null,
    DateTime? FollowUpAt = null,
    double? Latitude = null,
    double? Longitude = null);

public record CompanyVisitDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    Guid SellerUserId,
    DateTime VisitedAt,
    string Notes,
    string? Outcome,
    DateTime? FollowUpAt,
    double? Latitude,
    double? Longitude);

public record CompanyMapPointDto(
    Guid CompanyId,
    string Name,
    string Address,
    string Type,
    double Latitude,
    double Longitude);

public record CompanyVisitMapPointDto(
    Guid VisitId,
    Guid CompanyId,
    string CompanyName,
    Guid SellerUserId,
    DateTime VisitedAt,
    string Notes,
    double Latitude,
    double Longitude);

public interface ICompanyVisitService
{
    Task<CompanyVisitDto> CreateAsync(Guid sellerUserId, CreateCompanyVisitRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyVisitDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyMapPointDto>> GetMapPointsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyVisitMapPointDto>> GetVisitMapPointsAsync(CancellationToken cancellationToken = default);
}
