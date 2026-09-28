using BuildingBlocks.Application.CQRS;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Companies.Queries;

public record SearchCompaniesQuery(
    string SearchTerm,
    int Page = 1,
    int PageSize = 10
) : IQuery<PagedResult<CompanyListDto>>;

public class SearchCompaniesQueryHandler : IQueryHandler<SearchCompaniesQuery, PagedResult<CompanyListDto>>
{
    private readonly ICompanyRepository _companyRepository;

    public SearchCompaniesQueryHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<PagedResult<CompanyListDto>> HandleAsync(SearchCompaniesQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var items = await _companyRepository.GetListAsync(
            page,
            pageSize,
            query.SearchTerm,
            type: null,
            industry: null,
            cancellationToken
        );

        var totalCount = await _companyRepository.CountAsync(
            query.SearchTerm,
            type: null,
            industry: null,
            cancellationToken
        );

        var listDtos = items.Select(c => c.ToListDto()).ToList();

        return new PagedResult<CompanyListDto>(listDtos, totalCount, page, pageSize);
    }
}
