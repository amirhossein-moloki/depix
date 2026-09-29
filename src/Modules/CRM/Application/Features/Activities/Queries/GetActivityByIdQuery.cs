using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Activities.DTOs;
using Modules.CRM.Application.Features.Activities.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Activities.Queries;

public record GetActivityByIdQuery(Guid Id) : IQuery<ActivityDto>;

public class GetActivityByIdQueryHandler : IQueryHandler<GetActivityByIdQuery, ActivityDto>
{
    private readonly IActivityRepository _activityRepository;

    public GetActivityByIdQueryHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<ActivityDto> HandleAsync(GetActivityByIdQuery query, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(query.Id, cancellationToken);
        if (activity == null || activity.IsDeleted)
        {
            throw new EntityNotFoundException("Activity", query.Id);
        }

        return activity.ToDto();
    }
}
