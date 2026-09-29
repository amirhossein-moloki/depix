using BuildingBlocks.Application.CQRS;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.SalesNotes.DTOs;
using Modules.CRM.Application.Features.SalesNotes.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.SalesNotes.Queries;

public record GetSalesNotesQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? LeadId = null,
    Guid? ContactId = null,
    Guid? CompanyId = null,
    Guid? CreatedBy = null,
    string? Search = null
) : IQuery<PagedResult<SalesNoteListItemDto>>;

public class GetSalesNotesQueryHandler : IQueryHandler<GetSalesNotesQuery, PagedResult<SalesNoteListItemDto>>
{
    private readonly ISalesNoteRepository _salesNoteRepository;

    public GetSalesNotesQueryHandler(ISalesNoteRepository salesNoteRepository)
    {
        _salesNoteRepository = salesNoteRepository;
    }

    public async Task<PagedResult<SalesNoteListItemDto>> HandleAsync(GetSalesNotesQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : (query.PageSize > 100 ? 100 : query.PageSize);

        var totalCount = await _salesNoteRepository.CountAsync(
            query.LeadId,
            query.ContactId,
            query.CompanyId,
            query.CreatedBy,
            query.Search,
            cancellationToken
        );

        var salesNotes = await _salesNoteRepository.GetListAsync(
            page,
            pageSize,
            query.LeadId,
            query.ContactId,
            query.CompanyId,
            query.CreatedBy,
            query.Search,
            cancellationToken
        );

        var items = salesNotes.Select(sn => sn.ToListItemDto()).ToList();

        return new PagedResult<SalesNoteListItemDto>(items, totalCount, page, pageSize);
    }
}
