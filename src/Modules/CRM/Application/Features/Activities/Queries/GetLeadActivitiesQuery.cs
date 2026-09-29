using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Activities.DTOs;
using Modules.CRM.Application.Features.Activities.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Activities.Queries;

public record GetLeadActivitiesQuery(
    Guid LeadId,
    int Page = 1,
    int PageSize = 20
) : IQuery<LeadActivityHistoryDto>;

public class GetLeadActivitiesQueryHandler : IQueryHandler<GetLeadActivitiesQuery, LeadActivityHistoryDto>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ILeadRepository _leadRepository;

    public GetLeadActivitiesQueryHandler(
        IActivityRepository activityRepository,
        ILeadRepository leadRepository)
    {
        _activityRepository = activityRepository;
        _leadRepository = leadRepository;
    }

    public async Task<LeadActivityHistoryDto> HandleAsync(GetLeadActivitiesQuery query, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(query.LeadId, cancellationToken);
        if (lead == null)
        {
            throw new EntityNotFoundException("Lead", query.LeadId);
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 100 ? 100 : query.PageSize);

        var totalCount = await _activityRepository.CountAsync(
            leadId: query.LeadId,
            contactId: null,
            companyId: null,
            userId: null,
            type: null,
            result: null,
            fromDate: null,
            toDate: null,
            search: null,
            cancellationToken: cancellationToken
        );

        var activities = await _activityRepository.GetListAsync(
            page,
            pageSize,
            leadId: query.LeadId,
            contactId: null,
            companyId: null,
            userId: null,
            type: null,
            result: null,
            fromDate: null,
            toDate: null,
            search: null,
            cancellationToken: cancellationToken
        );

        var activityDtos = activities.Select(a => a.ToDto()).ToList();
        var lastInteractionAt = activities.OrderByDescending(a => a.OccurredAt).FirstOrDefault()?.OccurredAt;

        return new LeadActivityHistoryDto(
            query.LeadId,
            totalCount,
            lastInteractionAt,
            activityDtos
        );
    }
}
