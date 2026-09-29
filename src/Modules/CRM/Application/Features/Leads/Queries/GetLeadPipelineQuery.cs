using BuildingBlocks.Application.CQRS;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Queries;

public record GetLeadPipelineQuery(
    Guid? CompanyId = null,
    Guid? AssignedTo = null
) : IQuery<LeadPipelineDto>;

public class GetLeadPipelineQueryHandler : IQueryHandler<GetLeadPipelineQuery, LeadPipelineDto>
{
    private readonly ILeadRepository _leadRepository;

    public GetLeadPipelineQueryHandler(ILeadRepository leadRepository)
    {
        _leadRepository = leadRepository;
    }

    public async Task<LeadPipelineDto> HandleAsync(GetLeadPipelineQuery query, CancellationToken cancellationToken = default)
    {
        var allLeads = await _leadRepository.GetListAsync(
            1,
            1000,
            search: null,
            status: null,
            companyId: query.CompanyId,
            assignedTo: query.AssignedTo,
            source: null,
            fromDate: null,
            toDate: null,
            sortBy: "CreatedAt",
            sortDescending: true,
            cancellationToken: cancellationToken
        );

        var pipelineStages = new List<string> { "NEW", "CONTACTED", "QUALIFIED", "PROPOSAL", "WON", "DISQUALIFIED", "CONVERTED" };
        var stageDtos = new List<LeadPipelineStageDto>();

        foreach (var stage in pipelineStages)
        {
            var stageLeads = allLeads.Where(l => l.Status.Equals(stage, StringComparison.OrdinalIgnoreCase)).ToList();
            var count = stageLeads.Count;
            var totalValue = stageLeads.Sum(l => l.EstimatedValue ?? 0m);
            var leadDtos = stageLeads.Select(l => l.ToListItemDto()).ToList();

            stageDtos.Add(new LeadPipelineStageDto(stage, count, totalValue, leadDtos));
        }

        var totalLeadsCount = allLeads.Count;
        var totalPipelineValue = allLeads.Sum(l => l.EstimatedValue ?? 0m);

        return new LeadPipelineDto(stageDtos, totalLeadsCount, totalPipelineValue);
    }
}
