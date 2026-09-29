using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.SalesNotes.DTOs;
using Modules.CRM.Application.Features.SalesNotes.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.SalesNotes.Queries;

public record GetSalesNoteByIdQuery(Guid Id) : IQuery<SalesNoteDto>;

public class GetSalesNoteByIdQueryHandler : IQueryHandler<GetSalesNoteByIdQuery, SalesNoteDto>
{
    private readonly ISalesNoteRepository _salesNoteRepository;

    public GetSalesNoteByIdQueryHandler(ISalesNoteRepository salesNoteRepository)
    {
        _salesNoteRepository = salesNoteRepository;
    }

    public async Task<SalesNoteDto> HandleAsync(GetSalesNoteByIdQuery query, CancellationToken cancellationToken = default)
    {
        var salesNote = await _salesNoteRepository.GetByIdAsync(query.Id, cancellationToken);
        if (salesNote == null || salesNote.IsDeleted)
        {
            throw new EntityNotFoundException("SalesNote", query.Id);
        }

        return salesNote.ToDto();
    }
}
