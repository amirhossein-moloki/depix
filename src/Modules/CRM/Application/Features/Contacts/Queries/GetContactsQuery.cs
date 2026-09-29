using BuildingBlocks.Application.CQRS;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.Contacts.DTOs;
using Modules.CRM.Application.Features.Contacts.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Contacts.Queries;

public record GetContactsQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? CompanyId = null,
    string? Search = null
) : IQuery<PagedResult<ContactListDto>>;

public class GetContactsQueryHandler : IQueryHandler<GetContactsQuery, PagedResult<ContactListDto>>
{
    private readonly IContactRepository _contactRepository;

    public GetContactsQueryHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<PagedResult<ContactListDto>> HandleAsync(GetContactsQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var items = await _contactRepository.GetListAsync(
            page,
            pageSize,
            query.CompanyId,
            query.Search,
            cancellationToken
        );

        var totalCount = await _contactRepository.CountAsync(
            query.CompanyId,
            query.Search,
            cancellationToken
        );

        var listDtos = items.Select(c => c.ToListDto()).ToList();

        return new PagedResult<ContactListDto>(listDtos, totalCount, page, pageSize);
    }
}
