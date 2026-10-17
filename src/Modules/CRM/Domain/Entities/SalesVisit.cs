using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

public class SalesVisit : AuditableEntity, ISoftDelete
{
    public Guid CompanyId { get; private set; }
    public Guid SalespersonId { get; private set; }
    public DateTime VisitedAt { get; private set; } = DateTime.UtcNow;
    public string ReportText { get; private set; } = string.Empty;
    public string Result { get; private set; } = string.Empty;
    public DateTime? FollowUpAt { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? LocationSource { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private SalesVisit() { }

    public SalesVisit(
        Guid id,
        Guid companyId,
        Guid salespersonId,
        DateTime visitedAt,
        string reportText,
        string result,
        DateTime? followUpAt = null,
        double? latitude = null,
        double? longitude = null,
        string? locationSource = null) : base(id)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company ID is required.", nameof(companyId));

        if (salespersonId == Guid.Empty)
            throw new ArgumentException("Salesperson ID is required.", nameof(salespersonId));

        CompanyId = companyId;
        SalespersonId = salespersonId;
        VisitedAt = visitedAt == default ? DateTime.UtcNow : visitedAt;
        ReportText = reportText ?? string.Empty;
        Result = result ?? string.Empty;
        FollowUpAt = followUpAt;
        LocationSource = locationSource;

        SetLocation(latitude, longitude, locationSource);
    }

    public static SalesVisit Create(
        Guid companyId,
        Guid salespersonId,
        DateTime visitedAt,
        string reportText,
        string result,
        DateTime? followUpAt = null,
        double? latitude = null,
        double? longitude = null,
        string? locationSource = null)
    {
        return new SalesVisit(
            Guid.NewGuid(),
            companyId,
            salespersonId,
            visitedAt,
            reportText,
            result,
            followUpAt,
            latitude,
            longitude,
            locationSource);
    }

    public void SetLocation(double? latitude, double? longitude, string? source = null)
    {
        if (latitude.HasValue != longitude.HasValue)
        {
            throw new ArgumentException("Both latitude and longitude must be provided together, or both must be null.");
        }

        if (latitude.HasValue)
        {
            if (latitude.Value < -90.0 || latitude.Value > 90.0)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");
            }
            if (longitude!.Value < -180.0 || longitude.Value > 180.0)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");
            }
        }

        Latitude = latitude;
        Longitude = longitude;
        if (!string.IsNullOrWhiteSpace(source))
        {
            LocationSource = source.Trim();
        }
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UpdateInformation(
        string reportText,
        string result,
        DateTime visitedAt,
        DateTime? followUpAt = null,
        double? latitude = null,
        double? longitude = null,
        string? locationSource = null)
    {
        ReportText = reportText ?? ReportText;
        Result = result ?? Result;
        VisitedAt = visitedAt;
        FollowUpAt = followUpAt;
        SetLocation(latitude, longitude, locationSource);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
