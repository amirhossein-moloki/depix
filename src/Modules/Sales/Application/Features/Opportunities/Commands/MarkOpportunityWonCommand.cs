using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Mappings;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Opportunities.Commands;

public record MarkOpportunityWonCommand(
    Guid Id,
    DateTime? WonAt = null
) : ICommand<OpportunityDto>;

public class MarkOpportunityWonCommandHandler : ICommandHandler<MarkOpportunityWonCommand, OpportunityDto>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkOpportunityWonCommandHandler(
        IOpportunityRepository opportunityRepository,
        IUnitOfWork unitOfWork)
    {
        _opportunityRepository = opportunityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OpportunityDto> HandleAsync(MarkOpportunityWonCommand command, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(command.Id, cancellationToken);
        if (opportunity == null || opportunity.IsDeleted)
        {
            throw new EntityNotFoundException("Opportunity", command.Id);
        }

        opportunity.MarkAsWon(command.WonAt);
        _opportunityRepository.Update(opportunity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return opportunity.ToDto();
    }
}
