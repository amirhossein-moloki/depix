using BuildingBlocks.Domain.Models;

namespace Modules.Identity.Domain.Entities;

public enum UserApprovalStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

/// <summary>
/// Approval state for public registrations. Existing users without a row are treated as legacy-approved.
/// </summary>
public class UserApproval : Entity
{
    public Guid UserId { get; private set; }
    public UserApprovalStatus Status { get; private set; } = UserApprovalStatus.Pending;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedBy { get; private set; }

    private UserApproval() { }

    private UserApproval(Guid id, Guid userId) : base(id)
    {
        UserId = userId;
        Status = UserApprovalStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static UserApproval CreatePending(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID is required.", nameof(userId));
        return new UserApproval(Guid.NewGuid(), userId);
    }

    public void Approve(Guid reviewerId)
    {
        if (reviewerId == Guid.Empty) throw new ArgumentException("Reviewer ID is required.", nameof(reviewerId));
        Status = UserApprovalStatus.Approved;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewerId;
    }

    public void Reject(Guid reviewerId)
    {
        if (reviewerId == Guid.Empty) throw new ArgumentException("Reviewer ID is required.", nameof(reviewerId));
        Status = UserApprovalStatus.Rejected;
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewerId;
    }
}
