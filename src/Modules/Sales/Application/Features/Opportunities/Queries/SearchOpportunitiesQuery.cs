using BuildingBlocks.Application.CQRS;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Mappings;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Opportunities.Queries;

public record SearchOpportunitiesQuery(
    string? Search = null,
    string? Stage = null,
    string? Status = null,
    Guid? CustomerId = null,
    Guid? LeadId = null,
    Guid? CompanyId = null,
    int Page = 1,
    int PageSize = 10
) : IQuery<PagedResult<OpportunityListItemDto>>;

public class SearchOpportunitiesQueryHandler : IQueryHandler<SearchOpportunitiesQuery, PagedResult<OpportunityListItemDto>>
{
    private readonly IOpportunityRepository _opportunityRepository;

    public SearchOpportunitiesQueryHandler(IOpportunityRepository opportunityRepository)
    {
        _opportunityRepository = opportunityRepository;
    }

    public async Task<PagedResult<OpportunityListItemDto>> HandleAsync(SearchOpportunitiesQuery query, CancellationToken cancellationToken = default)
    {
        var filterParams = new OpportunityFilterParams(
            query.Page,
            query.PageSize,
            query.Search,
            query.Stage,
            query.Status,
            query.CustomerId,
            query.LeadId,
            query.CompanyId
        );

        var (items, totalCount) = await _opportunityRepository.GetFilteredAsync(filterParams, cancellationToken);
        var dtos = items.Select(o => o.ToListItemDto()).ToList();

        return new PagedResult<OpportunityListItemDto>(dtos, totalCount, query.Page, query.PageSize);
    }
}
