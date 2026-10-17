using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

public class CompanyVisit : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid SellerUserId { get; private set; }
    public DateTime VisitedAt { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public string? Outcome { get; private set; }
    public DateTime? FollowUpAt { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    private CompanyVisit() { }

    private CompanyVisit(Guid id, Guid companyId, Guid sellerUserId, DateTime visitedAt, string notes,
        string? outcome, DateTime? followUpAt, double? latitude, double? longitude) : base(id)
    {
        if (companyId == Guid.Empty) throw new ArgumentException("Company ID is required.", nameof(companyId));
        if (sellerUserId == Guid.Empty) throw new ArgumentException("Seller user ID is required.", nameof(sellerUserId));
        if (string.IsNullOrWhiteSpace(notes)) throw new ArgumentException("Visit notes are required.", nameof(notes));
        if (notes.Length > 5000) throw new ArgumentException("Visit notes cannot exceed 5000 characters.", nameof(notes));
        if ((latitude.HasValue != longitude.HasValue) || latitude is < -90 or > 90 || longitude is < -180 or > 180)
            throw new ArgumentException("Provide a valid latitude and longitude pair.");

        CompanyId = companyId;
        SellerUserId = sellerUserId;
        VisitedAt = visitedAt.Kind == DateTimeKind.Utc ? visitedAt : visitedAt.ToUniversalTime();
        Notes = notes.Trim();
        Outcome = string.IsNullOrWhiteSpace(outcome) ? null : outcome.Trim();
        FollowUpAt = followUpAt?.ToUniversalTime();
        Latitude = latitude;
        Longitude = longitude;
    }

    public static CompanyVisit Create(Guid companyId, Guid sellerUserId, DateTime visitedAt, string notes,
        string? outcome = null, DateTime? followUpAt = null, double? latitude = null, double? longitude = null)
        => new(Guid.NewGuid(), companyId, sellerUserId, visitedAt, notes, outcome, followUpAt, latitude, longitude);
}
