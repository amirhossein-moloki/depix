using BuildingBlocks.Application.CQRS;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Queries;

public record GetLeadsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Status = null,
    Guid? CompanyId = null,
    Guid? AssignedTo = null,
    string? Source = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SortBy = null,
    bool SortDescending = true
) : IQuery<PagedResult<LeadListItemDto>>;

public class GetLeadsQueryHandler : IQueryHandler<GetLeadsQuery, PagedResult<LeadListItemDto>>
{
    private readonly ILeadRepository _leadRepository;

    public GetLeadsQueryHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<PagedResult<LeadListItemDto>> HandleAsync(GetLeadsQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var items = await _leadRepository.GetListAsync(
            page,
            pageSize,
            query.Search,
            query.Status,
            query.CompanyId,
            query.AssignedTo,
            query.Source,
            query.FromDate,
            query.ToDate,
            query.SortBy,
            query.SortDescending,
            cancellationToken
        );

        var totalCount = await _leadRepository.CountAsync(
            query.Search,
            query.Status,
            query.CompanyId,
            query.AssignedTo,
            query.Source,
            query.FromDate,
            query.ToDate,
            cancellationToken
        );

        var listDtos = items.Select(l => l.ToListItemDto()).ToList();

        return new PagedResult<LeadListItemDto>(listDtos, totalCount, page, pageSize);
    }
}
