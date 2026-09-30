using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Mappings;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Opportunities.Queries;

public record GetOpportunityByIdQuery(Guid Id) : IQuery<OpportunityDto>;

public class GetOpportunityByIdQueryHandler : IQueryHandler<GetOpportunityByIdQuery, OpportunityDto>
{
    private readonly IOpportunityRepository _opportunityRepository;

    public GetOpportunityByIdQueryHandler(IOpportunityRepository opportunityRepository)
    {
        _opportunityRepository = opportunityRepository;
    }

    public async Task<OpportunityDto> HandleAsync(GetOpportunityByIdQuery query, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(query.Id, cancellationToken);
        if (opportunity == null || opportunity.IsDeleted)
        {
            throw new EntityNotFoundException("Opportunity", query.Id);
        }

        return opportunity.ToDto();
    }
}
