using BuildingBlocks.Application.CQRS;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Mappings;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Opportunities.Queries;

public record GetOpportunityPipelineQuery(
    Guid? CompanyId = null,
    Guid? CustomerId = null,
    Guid? AssignedTo = null
) : IQuery<OpportunityPipelineDto>;

public class GetOpportunityPipelineQueryHandler : IQueryHandler<GetOpportunityPipelineQuery, OpportunityPipelineDto>
{
    private readonly IOpportunityRepository _opportunityRepository;

    public GetOpportunityPipelineQueryHandler(IOpportunityRepository opportunityRepository)
    {
        _opportunityRepository = opportunityRepository;
    }

    public async Task<OpportunityPipelineDto> HandleAsync(GetOpportunityPipelineQuery query, CancellationToken cancellationToken = default)
    {
        var summaries = await _opportunityRepository.GetPipelineSummaryAsync(
            query.CompanyId,
            query.CustomerId,
            query.AssignedTo,
            cancellationToken
        );

        var stageDtos = summaries.Select(s => new PipelineStageGroupDto(
            s.Stage,
            s.Count,
            s.TotalValue,
            s.Items.Select(i => i.ToListItemDto()).ToList()
        )).ToList();

        var totalCount = stageDtos.Sum(s => s.Count);
        var totalPipelineValue = stageDtos.Sum(s => s.TotalValue);

        return new OpportunityPipelineDto(stageDtos, totalCount, totalPipelineValue);
    }
}
