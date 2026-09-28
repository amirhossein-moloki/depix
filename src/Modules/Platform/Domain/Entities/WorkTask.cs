using BuildingBlocks.Domain.Models;

namespace Modules.Platform.Domain.Entities;

public class WorkTask : AuditableAggregateRoot, ISoftDelete
{
    public Guid? AssignedTo { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Priority { get; private set; } = string.Empty; // LOW, MEDIUM, HIGH, URGENT
    public string Status { get; private set; } = string.Empty; // TODO, IN_PROGRESS, DONE, CANCELLED
    public DateOnly? Deadline { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private WorkTask() { }

    public WorkTask(Guid id, Guid? assignedTo, string title, string description, string priority, string status, DateOnly? deadline = null) : base(id)
    {
        AssignedTo = assignedTo;
        Title = title;
        Description = description;
        Priority = priority;
        Status = status;
        Deadline = deadline;
    }

    public static WorkTask Create(Guid? assignedTo, string title, string description, string priority = "MEDIUM", string status = "TODO", DateOnly? deadline = null)
    {
        return new WorkTask(Guid.NewGuid(), assignedTo, title, description, priority, status, deadline);
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
