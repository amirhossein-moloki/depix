using Modules.CRM.Application.Features.Activities.DTOs;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Application.Features.Activities.Mappings;

public static class ActivityMappingExtensions
{
    public static ActivityDto ToDto(this Activity activity)
    {
        return new ActivityDto(
            activity.Id,
            activity.LeadId,
            activity.UserId,
            activity.Type,
            activity.Subject,
            activity.Description,
            activity.Result,
            activity.QualityScore,
            activity.ContactId,
            activity.CompanyId,
            activity.OccurredAt,
            activity.FollowUpAt,
            activity.FollowUpNotes,
            activity.IsFollowUpRequired,
            activity.CreatedAt,
            activity.CreatedBy,
            activity.UpdatedAt,
            activity.UpdatedBy,
            activity.IsDeleted
        );
    }

    public static ActivityListItemDto ToListItemDto(this Activity activity)
    {
        return new ActivityListItemDto(
            activity.Id,
            activity.LeadId,
            activity.UserId,
            activity.Type,
            activity.Subject,
            activity.Result,
            activity.QualityScore,
            activity.OccurredAt,
            activity.FollowUpAt,
            activity.IsFollowUpRequired,
            activity.CreatedAt
        );
    }
}
