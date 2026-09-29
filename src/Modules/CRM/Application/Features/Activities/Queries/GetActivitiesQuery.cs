using BuildingBlocks.Application.CQRS;
using Modules.CRM.Application.Features.Activities.DTOs;
using Modules.CRM.Application.Features.Activities.Mappings;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Activities.Queries;

public record GetActivitiesQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? LeadId = null,
    Guid? ContactId = null,
    Guid? CompanyId = null,
    Guid? UserId = null,
    string? Type = null,
    string? Result = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Search = null
) : IQuery<PagedResult<ActivityListItemDto>>;

public class GetActivitiesQueryHandler : IQueryHandler<GetActivitiesQuery, PagedResult<ActivityListItemDto>>
{
    private readonly IActivityRepository _activityRepository;

    public GetActivitiesQueryHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<PagedResult<ActivityListItemDto>> HandleAsync(GetActivitiesQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

        var totalCount = await _activityRepository.CountAsync(
            query.LeadId,
            query.ContactId,
            query.CompanyId,
            query.UserId,
            query.Type,
            query.Result,
            query.FromDate,
            query.ToDate,
            query.Search,
            cancellationToken
        );

        var activities = await _activityRepository.GetListAsync(
            page,
            pageSize,
            query.LeadId,
            query.ContactId,
            query.CompanyId,
            query.UserId,
            query.Type,
            query.Result,
            query.FromDate,
            query.ToDate,
            query.Search,
            cancellationToken
        );

        var items = activities.Select(a => a.ToListItemDto()).ToList();

        return new PagedResult<ActivityListItemDto>(items, totalCount, page, pageSize);
    }
}
