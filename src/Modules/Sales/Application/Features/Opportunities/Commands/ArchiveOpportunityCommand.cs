using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Opportunities.Commands;

public record ArchiveOpportunityCommand(
    Guid Id,
    Guid? ArchivedBy = null
) : ICommand;

public class ArchiveOpportunityCommandHandler : ICommandHandler<ArchiveOpportunityCommand>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveOpportunityCommandHandler(
        IOpportunityRepository opportunityRepository,
        IUnitOfWork unitOfWork)
    {
        _opportunityRepository = opportunityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ArchiveOpportunityCommand command, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(command.Id, cancellationToken);
        if (opportunity == null || opportunity.IsDeleted)
        {
            throw new EntityNotFoundException("Opportunity", command.Id);
        }

        opportunity.Archive(command.ArchivedBy);
        _opportunityRepository.Update(opportunity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
