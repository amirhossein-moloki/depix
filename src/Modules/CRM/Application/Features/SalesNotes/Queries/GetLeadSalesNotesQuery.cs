using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.SalesNotes.DTOs;
using Modules.CRM.Application.Features.SalesNotes.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.SalesNotes.Queries;

public record GetLeadSalesNotesQuery(
    Guid LeadId,
    int Page = 1,
    int PageSize = 20
) : IQuery<LeadSalesContextDto>;

public class GetLeadSalesNotesQueryHandler : IQueryHandler<GetLeadSalesNotesQuery, LeadSalesContextDto>
{
    private readonly ISalesNoteRepository _salesNoteRepository;
    private readonly ILeadRepository _leadRepository;

    public GetLeadSalesNotesQueryHandler(
        ISalesNoteRepository salesNoteRepository,
        ILeadRepository leadRepository)
    {
        _salesNoteRepository = salesNoteRepository;
        _leadRepository = leadRepository;
    }

    public async Task<LeadSalesContextDto> HandleAsync(GetLeadSalesNotesQuery query, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(query.LeadId, cancellationToken);
        if (lead == null)
        {
            throw new EntityNotFoundException("Lead", query.LeadId);
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : (query.PageSize > 100 ? 100 : query.PageSize);

        var totalCount = await _salesNoteRepository.CountAsync(
            leadId: query.LeadId,
            contactId: null,
            companyId: null,
            createdBy: null,
            search: null,
            cancellationToken: cancellationToken
        );

        var salesNotes = await _salesNoteRepository.GetListAsync(
            page,
            pageSize,
            leadId: query.LeadId,
            contactId: null,
            companyId: null,
            createdBy: null,
            search: null,
            cancellationToken: cancellationToken
        );

        var noteDtos = salesNotes.Select(sn => sn.ToDto()).ToList();
        var latestProbability = salesNotes.OrderByDescending(sn => sn.CreatedAt).FirstOrDefault()?.Probability ?? 0;

        return new LeadSalesContextDto(
            query.LeadId,
            totalCount,
            latestProbability,
            noteDtos
        );
    }
}
