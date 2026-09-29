using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Queries;

public record GetLeadByIdQuery(Guid Id) : IQuery<LeadDto>;

public class GetLeadByIdQueryHandler : IQueryHandler<GetLeadByIdQuery, LeadDto>
{
    private readonly ILeadRepository _leadRepository;

    public GetLeadByIdQueryHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<LeadDto> HandleAsync(GetLeadByIdQuery query, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(query.Id, cancellationToken);
        if (lead == null)
        {
            throw new EntityNotFoundException("Lead", query.Id);
        }

        return lead.ToDto();
    }
}
