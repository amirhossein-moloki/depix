using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

public class SalesVisit : AuditableAggregateRoot, ISoftDelete
{
    public Guid CompanyId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid SalespersonId { get; private set; }
    public DateTime VisitDate { get; private set; }
    public string Report { get; private set; } = string.Empty;
    public string Result { get; private set; } = string.Empty; // SUCCESSFUL, UNSUCCESSFUL, FOLLOW_UP_REQUIRED, PENDING
    public DateTime? FollowUpAt { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public string? LocationSource { get; private set; } // GPS, MAP_PICKER, MANUAL

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private SalesVisit() { }

    public SalesVisit(
        Guid id,
        Guid companyId,
        Guid salespersonId,
        DateTime visitDate,
        string report,
        string result,
        Guid? customerId = null,
        DateTime? followUpAt = null,
        double? latitude = null,
        double? longitude = null,
        string? locationSource = null) : base(id)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company ID is required.", nameof(companyId));

        if (salespersonId == Guid.Empty)
            throw new ArgumentException("Salesperson ID is required.", nameof(salespersonId));

        if (string.IsNullOrWhiteSpace(report))
            throw new ArgumentException("Report text is required.", nameof(report));

        CompanyId = companyId;
        CustomerId = customerId;
        SalespersonId = salespersonId;
        VisitDate = visitDate;
        Report = report.Trim();
        Result = string.IsNullOrWhiteSpace(result) ? "SUCCESSFUL" : result.Trim().ToUpperInvariant();
        FollowUpAt = followUpAt;

        SetLocation(latitude, longitude, locationSource);
    }

    public static SalesVisit Create(
        Guid companyId,
        Guid salespersonId,
        DateTime visitDate,
        string report,
        string result,
        Guid? customerId = null,
        DateTime? followUpAt = null,
        double? latitude = null,
        double? longitude = null,
        string? locationSource = null)
    {
        return new SalesVisit(
            Guid.NewGuid(),
            companyId,
            salespersonId,
            visitDate,
            report,
            result,
            customerId,
            followUpAt,
            latitude,
            longitude,
            locationSource
        );
    }

    public void SetLocation(double? latitude, double? longitude, string? locationSource = null)
    {
        if (latitude.HasValue || longitude.HasValue)
        {
            if (!latitude.HasValue || !longitude.HasValue)
            {
                throw new ArgumentException("Both latitude and longitude must be provided together for visit location.");
            }
            if (latitude.Value < -90 || latitude.Value > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");
            }
            if (longitude.Value < -180 || longitude.Value > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");
            }
        }

        Latitude = latitude;
        Longitude = longitude;
        LocationSource = string.IsNullOrWhiteSpace(locationSource) ? (latitude.HasValue ? "GPS" : null) : locationSource.Trim().ToUpperInvariant();
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
