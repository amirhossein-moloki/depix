using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Mappings;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Opportunities.Commands;

public record CreateOpportunityCommand(
    string Title,
    string? Description = null,
    Guid? LeadId = null,
    Guid? CustomerId = null,
    Guid? CompanyId = null,
    Guid? ContactId = null,
    string? Stage = null,
    decimal ValueAmount = 0m,
    string ValueCurrency = "USD",
    int? Probability = null,
    DateOnly? ExpectedCloseDate = null,
    Guid? AssignedTo = null,
    string? Source = null
) : ICommand<OpportunityDto>;

public class CreateOpportunityCommandHandler : ICommandHandler<CreateOpportunityCommand, OpportunityDto>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOpportunityCommandHandler(
        IOpportunityRepository opportunityRepository,
        IUnitOfWork unitOfWork)
    {
        _opportunityRepository = opportunityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OpportunityDto> HandleAsync(CreateOpportunityCommand command, CancellationToken cancellationToken = default)
    {
        var money = Money.Create(command.ValueAmount, string.IsNullOrWhiteSpace(command.ValueCurrency) ? "USD" : command.ValueCurrency);
        var stage = string.IsNullOrWhiteSpace(command.Stage) ? OpportunityStage.New : command.Stage;

        var opportunity = Opportunity.Create(
            command.Title,
            command.LeadId,
            command.CustomerId,
            command.CompanyId,
            command.ContactId,
            stage,
            money,
            command.Probability,
            command.ExpectedCloseDate,
            command.AssignedTo,
            command.Source,
            command.Description
        );

        await _opportunityRepository.AddAsync(opportunity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return opportunity.ToDto();
    }
}
