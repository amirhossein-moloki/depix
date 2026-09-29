using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

public class Activity : AuditableEntity, ISoftDelete
{
    public Guid LeadId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Type { get; private set; } = string.Empty; // CALL, EMAIL, MEETING, MESSAGE, OTHER
    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Result { get; private set; } = string.Empty;
    public int QualityScore { get; private set; }
    public Guid? ContactId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;
    public DateTime? FollowUpAt { get; private set; }
    public string? FollowUpNotes { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public bool IsFollowUpRequired => FollowUpAt.HasValue;

    private Activity() { }

    public Activity(
        Guid id,
        Guid leadId,
        Guid? userId,
        string type,
        string subject,
        string description,
        string result,
        int qualityScore = 0,
        Guid? contactId = null,
        Guid? companyId = null,
        DateTime? occurredAt = null,
        DateTime? followUpAt = null,
        string? followUpNotes = null) : base(id)
    {
        LeadId = leadId;
        UserId = userId;
        Type = string.IsNullOrWhiteSpace(type) ? "CALL" : type.Trim().ToUpperInvariant();
        Subject = string.IsNullOrWhiteSpace(subject) ? (description ?? string.Empty) : subject.Trim();
        Description = description ?? string.Empty;
        Result = result ?? string.Empty;
        QualityScore = qualityScore;
        ContactId = contactId;
        CompanyId = companyId;
        OccurredAt = occurredAt ?? DateTime.UtcNow;
        FollowUpAt = followUpAt;
        FollowUpNotes = followUpNotes;
    }

    public static Activity Create(
        Guid leadId,
        Guid? userId,
        string type,
        string description,
        string result,
        int qualityScore)
    {
        return Create(leadId, userId, type, type, description, result, qualityScore);
    }

    public static Activity Create(
        Guid leadId,
        Guid? userId,
        string type,
        string subject,
        string description,
        string result,
        int qualityScore = 0,
        Guid? contactId = null,
        Guid? companyId = null,
        DateTime? occurredAt = null,
        DateTime? followUpAt = null,
        string? followUpNotes = null)
    {
        if (leadId == Guid.Empty)
        {
            throw new ArgumentException("Lead ID is required.", nameof(leadId));
        }

        return new Activity(
            Guid.NewGuid(),
            leadId,
            userId,
            type,
            subject,
            description,
            result,
            qualityScore,
            contactId,
            companyId,
            occurredAt,
            followUpAt,
            followUpNotes);
    }

    public void UpdateInformation(
        string type,
        string subject,
        string description,
        string result,
        int qualityScore = 0,
        Guid? contactId = null,
        Guid? companyId = null,
        DateTime? occurredAt = null,
        DateTime? followUpAt = null,
        string? followUpNotes = null)
    {
        Type = string.IsNullOrWhiteSpace(type) ? Type : type.Trim().ToUpperInvariant();
        Subject = string.IsNullOrWhiteSpace(subject) ? Subject : subject.Trim();
        Description = description ?? Description;
        Result = result ?? Result;
        QualityScore = qualityScore;
        ContactId = contactId ?? ContactId;
        CompanyId = companyId ?? CompanyId;
        if (occurredAt.HasValue) OccurredAt = occurredAt.Value;
        FollowUpAt = followUpAt;
        FollowUpNotes = followUpNotes;
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
