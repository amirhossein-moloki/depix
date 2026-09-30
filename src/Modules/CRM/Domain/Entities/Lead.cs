using BuildingBlocks.Domain.Models;
using Modules.CRM.Domain.Events;

namespace Modules.CRM.Domain.Entities;

public class Lead : AuditableAggregateRoot, ISoftDelete
{
    private readonly List<Activity> _activities = new();
    private readonly List<SalesNote> _salesNotes = new();

    public Guid CompanyId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public int Score { get; private set; }
    public decimal? EstimatedValue { get; private set; }
    public string? DisqualificationReason { get; private set; }
    public Guid? AssignedTo { get; private set; }
    public Guid? CustomerId { get; private set; }
    public DateTime? ConvertedAt { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public LeadResearch? Research { get; private set; }
    public IReadOnlyCollection<Activity> Activities => _activities.AsReadOnly();
    public IReadOnlyCollection<SalesNote> SalesNotes => _salesNotes.AsReadOnly();

    public static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "NEW",
        "CONTACTED",
        "QUALIFIED",
        "PROPOSAL",
        "WON",
        "DISQUALIFIED",
        "CONVERTED"
    };

    public static readonly HashSet<string> ConvertibleStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "QUALIFIED",
        "PROPOSAL",
        "WON"
    };

    private Lead() { }

    public Lead(
        Guid id,
        Guid companyId,
        string title,
        string source,
        string description = "",
        decimal? estimatedValue = null,
        Guid? contactId = null,
        Guid? assignedTo = null,
        int score = 0,
        string status = "NEW",
        string? disqualificationReason = null,
        Guid? customerId = null,
        DateTime? convertedAt = null) : base(id)
    {
        CompanyId = companyId;
        Title = string.IsNullOrWhiteSpace(title) ? source : title;
        Source = source ?? string.Empty;
        Description = description ?? string.Empty;
        EstimatedValue = estimatedValue;
        ContactId = contactId;
        AssignedTo = assignedTo;
        Score = score;
        Status = string.IsNullOrWhiteSpace(status) ? "NEW" : status.ToUpperInvariant();
        DisqualificationReason = disqualificationReason;
        CustomerId = customerId;
        ConvertedAt = convertedAt;
    }

    public static Lead Create(
        Guid companyId,
        string title,
        string source,
        string description = "",
        decimal? estimatedValue = null,
        Guid? contactId = null,
        Guid? assignedTo = null,
        int score = 0,
        string status = "NEW")
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException("Company ID is required.", nameof(companyId));
        }

        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? "NEW" : status.Trim().ToUpperInvariant();
        if (!ValidStatuses.Contains(normalizedStatus))
        {
            throw new ArgumentException($"Invalid lead status '{status}'.", nameof(status));
        }

        return new Lead(
            Guid.NewGuid(),
            companyId,
            title,
            source,
            description,
            estimatedValue,
            contactId,
            assignedTo,
            score,
            normalizedStatus);
    }

    public static Lead Create(Guid companyId, string source)
    {
        return Create(companyId, source, source, string.Empty, null, null, null, 0, "NEW");
    }

    public void UpdateInformation(
        string title,
        string source,
        string description,
        decimal? estimatedValue,
        Guid? contactId)
    {
        Title = string.IsNullOrWhiteSpace(title) ? Title : title;
        Source = source ?? Source;
        Description = description ?? Description;
        EstimatedValue = estimatedValue;
        ContactId = contactId;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public bool CanConvert()
    {
        return !IsDeleted && Status != "CONVERTED" && CustomerId == null && ConvertibleStatuses.Contains(Status);
    }

    public void ConvertToCustomer(Guid customerId)
    {
        if (Status == "CONVERTED" || CustomerId != null)
        {
            throw new InvalidOperationException("Lead has already been converted.");
        }

        if (!ConvertibleStatuses.Contains(Status))
        {
            throw new InvalidOperationException($"Lead in status '{Status}' cannot be converted. Only QUALIFIED, PROPOSAL, or WON leads can be converted.");
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID is required for conversion.", nameof(customerId));
        }

        var oldStatus = Status;
        Status = "CONVERTED";
        CustomerId = customerId;
        ConvertedAt = DateTime.UtcNow;

        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new LeadStatusChangedEvent(Id, oldStatus, "CONVERTED"));
        AddDomainEvent(new LeadConvertedEvent(Id, CompanyId, customerId));
    }

    public void ConvertToCustomer()
    {
        ConvertToCustomer(Guid.NewGuid());
    }

    public void Qualify()
    {
        if (Status == "QUALIFIED") return;

        var oldStatus = Status;
        Status = "QUALIFIED";
        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new LeadStatusChangedEvent(Id, oldStatus, "QUALIFIED"));
        AddDomainEvent(new LeadQualifiedEvent(Id, CompanyId));
    }

    public void Disqualify(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Disqualification reason is required.", nameof(reason));
        }

        var oldStatus = Status;
        Status = "DISQUALIFIED";
        DisqualificationReason = reason;
        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new LeadStatusChangedEvent(Id, oldStatus, "DISQUALIFIED"));
        AddDomainEvent(new LeadDisqualifiedEvent(Id, CompanyId, reason));
    }

    public void ChangeStatus(string newStatus, string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(newStatus))
        {
            throw new ArgumentException("Status cannot be empty.", nameof(newStatus));
        }

        var normalizedStatus = newStatus.Trim().ToUpperInvariant();
        if (!ValidStatuses.Contains(normalizedStatus))
        {
            throw new ArgumentException($"Invalid status transition target '{newStatus}'.", nameof(newStatus));
        }

        if (normalizedStatus == "DISQUALIFIED")
        {
            Disqualify(reason ?? "Disqualified");
            return;
        }

        if (normalizedStatus == "QUALIFIED")
        {
            Qualify();
            return;
        }

        if (normalizedStatus == "CONVERTED")
        {
            ConvertToCustomer(CustomerId ?? Guid.NewGuid());
            return;
        }

        if (Status == normalizedStatus) return;

        var oldStatus = Status;
        Status = normalizedStatus;
        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new LeadStatusChangedEvent(Id, oldStatus, normalizedStatus));
    }

    public void AssignToUser(Guid? userId)
    {
        if (AssignedTo == userId) return;

        AssignedTo = userId;
        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new LeadAssignedEvent(Id, userId));
    }

    public void UpdateScore(int score)
    {
        if (score < 0)
        {
            throw new ArgumentException("Lead score cannot be negative.", nameof(score));
        }

        Score = score;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void SetResearch(LeadResearch research)
    {
        Research = research;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AddActivity(Activity activity)
    {
        _activities.Add(activity);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AddSalesNote(SalesNote note)
    {
        _salesNotes.Add(note);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UpdateStatus(string status)
    {
        ChangeStatus(status);
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
