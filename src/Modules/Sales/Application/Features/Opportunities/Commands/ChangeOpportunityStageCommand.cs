using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Mappings;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Opportunities.Commands;

public record ChangeOpportunityStageCommand(
    Guid Id,
    string Stage,
    int? Probability = null
) : ICommand<OpportunityDto>;

public class ChangeOpportunityStageCommandHandler : ICommandHandler<ChangeOpportunityStageCommand, OpportunityDto>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeOpportunityStageCommandHandler(
        IOpportunityRepository opportunityRepository,
        IUnitOfWork unitOfWork)
    {
        _opportunityRepository = opportunityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OpportunityDto> HandleAsync(ChangeOpportunityStageCommand command, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(command.Id, cancellationToken);
        if (opportunity == null || opportunity.IsDeleted)
        {
            throw new EntityNotFoundException("Opportunity", command.Id);
        }

        opportunity.MoveToStage(command.Stage, command.Probability);
        _opportunityRepository.Update(opportunity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return opportunity.ToDto();
    }
}
