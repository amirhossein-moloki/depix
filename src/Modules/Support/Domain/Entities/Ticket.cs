using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.Models;
using Modules.Support.Domain.Constants;
using Modules.Support.Domain.Events;

namespace Modules.Support.Domain.Entities;

public class Ticket : AuditableAggregateRoot, ISoftDelete
{
    private readonly List<TicketComment> _comments = new();

    public string TicketNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Priority { get; private set; } = TicketPriority.Normal;
    public string Status { get; private set; } = TicketStatus.Open;
    public string Category { get; private set; } = TicketCategory.General;
    public Guid? AssignedToUserId { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? DueAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? Resolution { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public IReadOnlyCollection<TicketComment> Comments => _comments.AsReadOnly();

    private Ticket() { }

    private Ticket(
        Guid id,
        string ticketNumber,
        Guid customerId,
        Guid? projectId,
        Guid? contactId,
        string subject,
        string description,
        string priority,
        string category,
        Guid? assignedToUserId,
        DateTime? dueAt) : base(id)
    {
        if (customerId == Guid.Empty)
            throw new BusinessRuleException("A ticket must be associated with a valid customer.");

        if (string.IsNullOrWhiteSpace(subject))
            throw new BusinessRuleException("Ticket subject is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleException("Ticket description is required.");

        var normPriority = string.IsNullOrWhiteSpace(priority) ? TicketPriority.Normal : priority.ToUpperInvariant();
        if (!TicketPriority.IsValid(normPriority))
            throw new BusinessRuleException($"Invalid ticket priority '{priority}'.");

        var normCategory = string.IsNullOrWhiteSpace(category) ? TicketCategory.General : category.ToUpperInvariant();
        if (!TicketCategory.IsValid(normCategory))
            throw new BusinessRuleException($"Invalid ticket category '{category}'.");

        TicketNumber = ticketNumber;
        CustomerId = customerId;
        ProjectId = projectId;
        ContactId = contactId;
        Subject = subject;
        Description = description;
        Priority = normPriority;
        Category = normCategory;
        AssignedToUserId = assignedToUserId;
        OpenedAt = DateTime.UtcNow;
        DueAt = dueAt;
        Status = TicketStatus.Open;

        AddDomainEvent(new TicketCreatedEvent(Id, TicketNumber, CustomerId));
    }

    public static Ticket Create(
        string ticketNumber,
        Guid customerId,
        Guid? projectId,
        Guid? contactId,
        string subject,
        string description,
        string priority = TicketPriority.Normal,
        string category = TicketCategory.General,
        Guid? assignedToUserId = null,
        DateTime? dueAt = null)
    {
        return new Ticket(
            Guid.NewGuid(),
            ticketNumber,
            customerId,
            projectId,
            contactId,
            subject,
            description,
            priority,
            category,
            assignedToUserId,
            dueAt);
    }

    public void UpdateDetails(
        string subject,
        string description,
        string category,
        Guid? projectId,
        Guid? contactId,
        DateTime? dueAt)
    {
        EnsureActive("update details");

        if (string.IsNullOrWhiteSpace(subject))
            throw new BusinessRuleException("Ticket subject is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessRuleException("Ticket description is required.");

        var normCategory = string.IsNullOrWhiteSpace(category) ? Category : category.ToUpperInvariant();
        if (!TicketCategory.IsValid(normCategory))
            throw new BusinessRuleException($"Invalid ticket category '{category}'.");

        Subject = subject;
        Description = description;
        Category = normCategory;
        ProjectId = projectId;
        ContactId = contactId;
        DueAt = dueAt;

        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AssignTo(Guid? userId, string? assignedByName = null)
    {
        EnsureActive("assign user");

        if (AssignedToUserId == userId)
            return;

        AssignedToUserId = userId;
        UpdateTimestamp(DateTime.UtcNow);

        var message = userId.HasValue
            ? $"Ticket assigned to user {userId.Value}."
            : "Ticket unassigned.";

        AddCommentInternal(userId, assignedByName ?? "System", message, CommentType.Assignment);
        AddDomainEvent(new TicketAssignedEvent(Id, AssignedToUserId));
    }

    public void ChangePriority(string newPriority, Guid? userId = null, string? updatedByName = null)
    {
        EnsureActive("change priority");

        var normPriority = (newPriority ?? string.Empty).ToUpperInvariant();
        if (!TicketPriority.IsValid(normPriority))
            throw new BusinessRuleException($"Invalid ticket priority '{newPriority}'.");

        if (Priority.Equals(normPriority, StringComparison.OrdinalIgnoreCase))
            return;

        var oldPriority = Priority;
        Priority = normPriority;
        UpdateTimestamp(DateTime.UtcNow);

        AddCommentInternal(userId, updatedByName ?? "System", $"Priority changed from {oldPriority} to {Priority}.", CommentType.PriorityChange);
        AddDomainEvent(new TicketPriorityChangedEvent(Id, oldPriority, Priority));
    }

    public void ChangeStatus(string newStatus, Guid? userId = null, string? updatedByName = null)
    {
        var normStatus = (newStatus ?? string.Empty).ToUpperInvariant();
        if (!TicketStatus.IsValid(normStatus))
            throw new BusinessRuleException($"Invalid ticket status '{newStatus}'.");

        if (Status.Equals(normStatus, StringComparison.OrdinalIgnoreCase))
            return;

        if (Status == TicketStatus.Cancelled)
            throw new BusinessRuleException("Cannot change status of a cancelled ticket.");

        if (Status == TicketStatus.Closed && normStatus != TicketStatus.Open && normStatus != TicketStatus.InProgress)
            throw new BusinessRuleException("Cannot directly change status of a closed ticket without reopening.");

        if (normStatus == TicketStatus.Resolved && string.IsNullOrWhiteSpace(Resolution))
            throw new BusinessRuleException("Ticket cannot be moved to Resolved without resolution details.");

        var oldStatus = Status;
        Status = normStatus;
        UpdateTimestamp(DateTime.UtcNow);

        if (Status == TicketStatus.Closed)
        {
            ClosedAt ??= DateTime.UtcNow;
            AddDomainEvent(new TicketClosedEvent(Id, ClosedAt.Value));
        }

        AddCommentInternal(userId, updatedByName ?? "System", $"Status changed from {oldStatus} to {Status}.", CommentType.StatusChange);
        AddDomainEvent(new TicketStatusChangedEvent(Id, oldStatus, Status));
    }

    public void Resolve(string resolution, DateTime? resolvedAt = null, Guid? userId = null, string? resolvedByName = null)
    {
        EnsureActive("resolve ticket");

        if (string.IsNullOrWhiteSpace(resolution))
            throw new BusinessRuleException("Resolution details are required to resolve a ticket.");

        var oldStatus = Status;
        Resolution = resolution;
        ResolvedAt = resolvedAt ?? DateTime.UtcNow;
        Status = TicketStatus.Resolved;
        UpdateTimestamp(DateTime.UtcNow);

        AddCommentInternal(userId, resolvedByName ?? "System", $"Ticket resolved: {resolution}", CommentType.Resolution);
        AddDomainEvent(new TicketResolvedEvent(Id, Resolution, ResolvedAt.Value));
        AddDomainEvent(new TicketStatusChangedEvent(Id, oldStatus, Status));
    }

    public void Close(Guid? userId = null, string? closedByName = null)
    {
        if (Status == TicketStatus.Closed)
            return;

        if (Status == TicketStatus.Cancelled)
            throw new BusinessRuleException("Cannot close a cancelled ticket.");

        var oldStatus = Status;
        ClosedAt = DateTime.UtcNow;
        Status = TicketStatus.Closed;
        UpdateTimestamp(DateTime.UtcNow);

        AddCommentInternal(userId, closedByName ?? "System", "Ticket closed.", CommentType.StatusChange);
        AddDomainEvent(new TicketClosedEvent(Id, ClosedAt.Value));
        AddDomainEvent(new TicketStatusChangedEvent(Id, oldStatus, Status));
    }

    public void Cancel(string? reason = null, Guid? userId = null, string? cancelledByName = null)
    {
        if (Status == TicketStatus.Cancelled)
            return;

        if (Status == TicketStatus.Closed)
            throw new BusinessRuleException("Cannot cancel a closed ticket.");

        var oldStatus = Status;
        Status = TicketStatus.Cancelled;
        UpdateTimestamp(DateTime.UtcNow);

        var msg = string.IsNullOrWhiteSpace(reason) ? "Ticket cancelled." : $"Ticket cancelled. Reason: {reason}";
        AddCommentInternal(userId, cancelledByName ?? "System", msg, CommentType.StatusChange);
        AddDomainEvent(new TicketStatusChangedEvent(Id, oldStatus, Status));
    }

    public void Reopen(string? reason = null, Guid? userId = null, string? reopenedByName = null)
    {
        if (Status == TicketStatus.Cancelled)
            throw new BusinessRuleException("Cancelled tickets cannot be reopened.");

        if (Status != TicketStatus.Resolved && Status != TicketStatus.Closed)
            throw new BusinessRuleException("Only resolved or closed tickets can be reopened.");

        var oldStatus = Status;
        Status = TicketStatus.Open;
        ResolvedAt = null;
        ClosedAt = null;
        UpdateTimestamp(DateTime.UtcNow);

        var msg = string.IsNullOrWhiteSpace(reason) ? "Ticket reopened." : $"Ticket reopened. Reason: {reason}";
        AddCommentInternal(userId, reopenedByName ?? "System", msg, CommentType.Reopened);
        AddDomainEvent(new TicketReopenedEvent(Id));
        AddDomainEvent(new TicketStatusChangedEvent(Id, oldStatus, Status));
    }

    public TicketComment AddComment(Guid? authorUserId, string authorName, string message, string commentType = CommentType.Comment)
    {
        if (IsDeleted)
            throw new BusinessRuleException("Cannot add comment to a deleted ticket.");

        if (Status == TicketStatus.Cancelled)
            throw new BusinessRuleException("Cannot add comment to a cancelled ticket.");

        var comment = AddCommentInternal(authorUserId, authorName, message, commentType);
        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new TicketCommentAddedEvent(Id, comment.Id));
        return comment;
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

    private TicketComment AddCommentInternal(Guid? authorUserId, string authorName, string message, string commentType)
    {
        var comment = TicketComment.Create(Id, authorUserId, authorName, message, commentType);
        _comments.Add(comment);
        return comment;
    }

    private void EnsureActive(string actionName)
    {
        if (IsDeleted)
            throw new BusinessRuleException($"Cannot {actionName} on a deleted ticket.");

        if (Status == TicketStatus.Closed)
            throw new BusinessRuleException($"Cannot {actionName} on a closed ticket.");

        if (Status == TicketStatus.Cancelled)
            throw new BusinessRuleException($"Cannot {actionName} on a cancelled ticket.");
    }
}
