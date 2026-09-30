using BuildingBlocks.Application.CQRS;
using Modules.Customer.Application.DTOs;
using Modules.Customer.Application.Mappings;
using Modules.Customer.Domain.Repositories;

namespace Modules.Customer.Application.Queries;

public record GetCustomersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Status = null,
    Guid? CompanyId = null,
    Guid? AssignedTo = null,
    string? SortBy = null,
    bool SortDescending = false) : IQuery<PagedResult<CustomerListItemDto>>;

public class GetCustomersQueryHandler : IQueryHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResult<CustomerListItemDto>> HandleAsync(GetCustomersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _customerRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.Status,
            query.CompanyId,
            query.AssignedTo,
            query.SortBy,
            query.SortDescending,
            cancellationToken);

        var dtos = items.Select(c => c.ToListItemDto()).ToList();
        return new PagedResult<CustomerListItemDto>(dtos, totalCount, query.Page, query.PageSize);
    }
}
