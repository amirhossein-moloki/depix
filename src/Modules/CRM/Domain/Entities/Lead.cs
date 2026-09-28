using BuildingBlocks.Domain.Models;
using Modules.CRM.Domain.Events;

namespace Modules.CRM.Domain.Entities;

public class Lead : AuditableAggregateRoot, ISoftDelete
{
    private readonly List<Activity> _activities = new();
    private readonly List<SalesNote> _salesNotes = new();

    public Guid CompanyId { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public int Score { get; private set; }
    public Guid? AssignedTo { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public LeadResearch? Research { get; private set; }
    public IReadOnlyCollection<Activity> Activities => _activities.AsReadOnly();
    public IReadOnlyCollection<SalesNote> SalesNotes => _salesNotes.AsReadOnly();

    private Lead() { }

    public Lead(Guid id, Guid companyId, string source, string status, int score, Guid? assignedTo = null) : base(id)
    {
        CompanyId = companyId;
        Source = source;
        Status = status;
        Score = score;
        AssignedTo = assignedTo;
    }

    public static Lead Create(Guid companyId, string source, string status = "NEW", int score = 0, Guid? assignedTo = null)
    {
        return new Lead(Guid.NewGuid(), companyId, source, status, score, assignedTo);
    }

    public void ConvertToCustomer()
    {
        if (Status == "CONVERTED") return;

        Status = "CONVERTED";
        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new LeadConvertedEvent(Id, CompanyId));
    }

    public void Qualify()
    {
        Status = "QUALIFIED";
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void Disqualify()
    {
        Status = "DISQUALIFIED";
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AssignToUser(Guid userId)
    {
        AssignedTo = userId;
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
        Status = status;
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
