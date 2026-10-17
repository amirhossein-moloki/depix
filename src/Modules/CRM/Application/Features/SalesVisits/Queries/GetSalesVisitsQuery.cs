using BuildingBlocks.Application.CQRS;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.SalesVisits.DTOs;
using Modules.CRM.Application.Features.SalesVisits.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.SalesVisits.Queries;

public record GetSalesVisitsQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? CompanyId = null,
    Guid? SalespersonId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? ResultFilter = null
) : IQuery<PagedResult<SalesVisitListDto>>;

public class GetSalesVisitsQueryHandler : IQueryHandler<GetSalesVisitsQuery, PagedResult<SalesVisitListDto>>
{
    private readonly ISalesVisitRepository _salesVisitRepository;

    public GetSalesVisitsQueryHandler(ISalesVisitRepository salesVisitRepository)
    {
        _salesVisitRepository = salesVisitRepository;
    }

    public async Task<PagedResult<SalesVisitListDto>> HandleAsync(GetSalesVisitsQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

        var (items, totalCount) = await _salesVisitRepository.GetPagedAsync(
            page,
            pageSize,
            query.CompanyId,
            query.SalespersonId,
            query.FromDate,
            query.ToDate,
            query.ResultFilter,
            cancellationToken
        );

        var listDtos = items.Select(v => v.ToListDto()).ToList();
        return new PagedResult<SalesVisitListDto>(listDtos, totalCount, page, pageSize);
    }
}
