using BuildingBlocks.Domain.Models;

namespace Modules.Support.Domain.Entities;

public class Ticket : AuditableAggregateRoot, ISoftDelete
{
    public Guid CustomerId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Priority { get; private set; } = string.Empty; // LOW, MEDIUM, HIGH, URGENT
    public string Status { get; private set; } = string.Empty; // OPEN, IN_PROGRESS, RESOLVED, CLOSED

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private Ticket() { }

    public Ticket(Guid id, Guid customerId, Guid? projectId, string title, string description, string priority, string status) : base(id)
    {
        CustomerId = customerId;
        ProjectId = projectId;
        Title = title;
        Description = description;
        Priority = priority;
        Status = status;
    }

    public static Ticket Create(Guid customerId, Guid? projectId, string title, string description, string priority = "MEDIUM", string status = "OPEN")
    {
        return new Ticket(Guid.NewGuid(), customerId, projectId, title, description, priority, status);
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
