using BuildingBlocks.Application.CQRS;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Queries;

public record SearchLeadsQuery(
    string? Search = null,
    string? Status = null,
    Guid? CompanyId = null,
    int Page = 1,
    int PageSize = 10
) : IQuery<PagedResult<LeadListItemDto>>;

public class SearchLeadsQueryHandler : IQueryHandler<SearchLeadsQuery, PagedResult<LeadListItemDto>>
{
    private readonly ILeadRepository _leadRepository;

    public SearchLeadsQueryHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<PagedResult<LeadListItemDto>> HandleAsync(SearchLeadsQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var items = await _leadRepository.GetListAsync(
            page,
            pageSize,
            query.Search,
            query.Status,
            query.CompanyId,
            null,
            null,
            null,
            null,
            "CreatedAt",
            true,
            cancellationToken
        );

        var totalCount = await _leadRepository.CountAsync(
            query.Search,
            query.Status,
            query.CompanyId,
            null,
            null,
            null,
            null,
            cancellationToken
        );

        var listDtos = items.Select(l => l.ToListItemDto()).ToList();

        return new PagedResult<LeadListItemDto>(listDtos, totalCount, page, pageSize);
    }
}
