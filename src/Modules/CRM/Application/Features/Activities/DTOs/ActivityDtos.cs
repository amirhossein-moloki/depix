using BuildingBlocks.Application.CQRS;

namespace Modules.CRM.Application.Features.Activities.DTOs;

public record ActivityDto(
    Guid Id,
    Guid LeadId,
    Guid? UserId,
    string Type,
    string Subject,
    string Description,
    string Result,
    int QualityScore,
    Guid? ContactId,
    Guid? CompanyId,
    DateTime OccurredAt,
    DateTime? FollowUpAt,
    string? FollowUpNotes,
    bool IsFollowUpRequired,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    bool IsDeleted
);

public record ActivityListItemDto(
    Guid Id,
    Guid LeadId,
    Guid? UserId,
    string Type,
    string Subject,
    string Result,
    int QualityScore,
    DateTime OccurredAt,
    DateTime? FollowUpAt,
    bool IsFollowUpRequired,
    DateTime CreatedAt
);

public record LeadActivityHistoryDto(
    Guid LeadId,
    int TotalActivitiesCount,
    DateTime? LastInteractionAt,
    IReadOnlyList<ActivityDto> Activities
);

public record CreateActivityRequest(
    Guid LeadId,
    Guid? UserId,
    string Type,
    string Subject,
    string Description,
    string Result,
    int QualityScore = 0,
    Guid? ContactId = null,
    Guid? CompanyId = null,
    DateTime? OccurredAt = null,
    DateTime? FollowUpAt = null,
    string? FollowUpNotes = null
);

public record UpdateActivityRequest(
    string Type,
    string Subject,
    string Description,
    string Result,
    int QualityScore = 0,
    Guid? ContactId = null,
    Guid? CompanyId = null,
    DateTime? OccurredAt = null,
    DateTime? FollowUpAt = null,
    string? FollowUpNotes = null
);
