using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Mappings;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Opportunities.Commands;

public record UpdateOpportunityCommand(
    Guid Id,
    string Title,
    string? Description = null,
    decimal ValueAmount = 0m,
    string ValueCurrency = "USD",
    int Probability = 10,
    DateOnly ExpectedCloseDate = default,
    string? Source = null,
    Guid? ContactId = null
) : ICommand<OpportunityDto>;

public class UpdateOpportunityCommandHandler : ICommandHandler<UpdateOpportunityCommand, OpportunityDto>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOpportunityCommandHandler(
        IOpportunityRepository opportunityRepository,
        IUnitOfWork unitOfWork)
    {
        _opportunityRepository = opportunityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OpportunityDto> HandleAsync(UpdateOpportunityCommand command, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(command.Id, cancellationToken);
        if (opportunity == null || opportunity.IsDeleted)
        {
            throw new EntityNotFoundException("Opportunity", command.Id);
        }

        var money = Money.Create(command.ValueAmount, string.IsNullOrWhiteSpace(command.ValueCurrency) ? "USD" : command.ValueCurrency);

        opportunity.UpdateDetails(
            command.Title,
            command.Description,
            money,
            command.Probability,
            command.ExpectedCloseDate,
            command.Source,
            command.ContactId
        );

        _opportunityRepository.Update(opportunity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return opportunity.ToDto();
    }
}
