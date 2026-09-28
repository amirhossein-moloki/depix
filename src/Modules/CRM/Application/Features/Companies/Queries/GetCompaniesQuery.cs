using BuildingBlocks.Application.CQRS;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Companies.Queries;

public record GetCompaniesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Type = null,
    string? Industry = null
) : IQuery<PagedResult<CompanyListDto>>;

public class GetCompaniesQueryHandler : IQueryHandler<GetCompaniesQuery, PagedResult<CompanyListDto>>
{
    private readonly ICompanyRepository _companyRepository;

    public GetCompaniesQueryHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<PagedResult<CompanyListDto>> HandleAsync(GetCompaniesQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var items = await _companyRepository.GetListAsync(
            page,
            pageSize,
            query.Search,
            query.Type,
            query.Industry,
            cancellationToken
        );

        var totalCount = await _companyRepository.CountAsync(
            query.Search,
            query.Type,
            query.Industry,
            cancellationToken
        );

        var listDtos = items.Select(c => c.ToListDto()).ToList();

        return new PagedResult<CompanyListDto>(listDtos, totalCount, page, pageSize);
    }
}
