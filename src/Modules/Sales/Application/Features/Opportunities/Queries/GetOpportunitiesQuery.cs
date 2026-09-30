using BuildingBlocks.Application.CQRS;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Mappings;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Opportunities.Queries;

public record GetOpportunitiesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Stage = null,
    string? Status = null,
    Guid? CustomerId = null,
    Guid? LeadId = null,
    Guid? CompanyId = null,
    Guid? AssignedTo = null,
    decimal? MinValue = null,
    decimal? MaxValue = null,
    DateOnly? FromCloseDate = null,
    DateOnly? ToCloseDate = null,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PagedResult<OpportunityListItemDto>>;

public class GetOpportunitiesQueryHandler : IQueryHandler<GetOpportunitiesQuery, PagedResult<OpportunityListItemDto>>
{
    private readonly IOpportunityRepository _opportunityRepository;

    public GetOpportunitiesQueryHandler(IOpportunityRepository opportunityRepository)
    {
        _opportunityRepository = opportunityRepository;
    }

    public async Task<PagedResult<OpportunityListItemDto>> HandleAsync(GetOpportunitiesQuery query, CancellationToken cancellationToken = default)
    {
        var filterParams = new OpportunityFilterParams(
            query.Page,
            query.PageSize,
            query.Search,
            query.Stage,
            query.Status,
            query.CustomerId,
            query.LeadId,
            query.CompanyId,
            query.AssignedTo,
            query.MinValue,
            query.MaxValue,
            query.FromCloseDate,
            query.ToCloseDate,
            query.SortBy,
            query.SortDescending
        );

        var (items, totalCount) = await _opportunityRepository.GetFilteredAsync(filterParams, cancellationToken);
        var dtos = items.Select(o => o.ToListItemDto()).ToList();

        return new PagedResult<OpportunityListItemDto>(dtos, totalCount, query.Page, query.PageSize);
    }
}
