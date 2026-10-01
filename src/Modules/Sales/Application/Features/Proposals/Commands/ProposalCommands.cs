using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Sales.Application.Features.Proposals.DTOs;
using Modules.Sales.Application.Features.Proposals.Mappings;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Proposals.Commands;

public record CreateProposalCommand(
    Guid OpportunityId,
    string Title,
    DateOnly ValidUntil,
    string Version = "1.0",
    string? Description = null,
    Guid? CustomerId = null,
    Guid? CompanyId = null,
    string? Notes = null,
    string Currency = "USD"
) : ICommand<ProposalDto>;

public class CreateProposalCommandHandler : ICommandHandler<CreateProposalCommand, ProposalDto>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProposalCommandHandler(
        IOpportunityRepository opportunityRepository,
        IProposalRepository proposalRepository,
        IUnitOfWork unitOfWork)
    {
        _opportunityRepository = opportunityRepository;
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProposalDto> HandleAsync(CreateProposalCommand command, CancellationToken cancellationToken = default)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(command.OpportunityId, cancellationToken);
        if (opportunity == null || opportunity.IsDeleted)
        {
            throw new EntityNotFoundException("Opportunity", command.OpportunityId);
        }

        var customerId = command.CustomerId ?? opportunity.CustomerId;
        var companyId = command.CompanyId ?? opportunity.CompanyId;
        var currency = string.IsNullOrWhiteSpace(command.Currency) ? (opportunity.Value?.Currency ?? "USD") : command.Currency;

        var proposal = Proposal.Create(
            command.OpportunityId,
            command.Title,
            command.ValidUntil,
            command.Version,
            command.Description,
            customerId,
            companyId,
            command.Notes,
            currency
        );

        opportunity.AddProposal(proposal);
        await _proposalRepository.AddAsync(proposal, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return proposal.ToDto();
    }
}

public record UpdateProposalCommand(
    Guid ProposalId,
    string Title,
    string Description,
    DateOnly ValidUntil,
    string Version,
    string? Notes = null
) : ICommand<ProposalDto>;

public class UpdateProposalCommandHandler : ICommandHandler<UpdateProposalCommand, ProposalDto>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProposalCommandHandler(IProposalRepository proposalRepository, IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProposalDto> HandleAsync(UpdateProposalCommand command, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId, cancellationToken);
        if (proposal == null || proposal.IsDeleted)
        {
            throw new EntityNotFoundException("Proposal", command.ProposalId);
        }

        proposal.UpdateDetails(
            command.Title,
            command.Description,
            command.ValidUntil,
            command.Version,
            command.Notes
        );

        _proposalRepository.Update(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return proposal.ToDto();
    }
}

public record AddProposalItemCommand(
    Guid ProposalId,
    string Name,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount = 0m
) : ICommand<ProposalDto>;

public class AddProposalItemCommandHandler : ICommandHandler<AddProposalItemCommand, ProposalDto>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddProposalItemCommandHandler(IProposalRepository proposalRepository, IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProposalDto> HandleAsync(AddProposalItemCommand command, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId, cancellationToken);
        if (proposal == null || proposal.IsDeleted)
        {
            throw new EntityNotFoundException("Proposal", command.ProposalId);
        }

        var currency = proposal.Currency ?? "USD";
        var unitPrice = Money.Create(command.UnitPrice, currency);
        var discount = Money.Create(command.Discount, currency);

        proposal.AddItem(command.Name, command.Description, command.Quantity, unitPrice, discount);

        _proposalRepository.Update(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return proposal.ToDto();
    }
}

public record UpdateProposalItemCommand(
    Guid ProposalId,
    Guid ItemId,
    string Name,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount = 0m
) : ICommand<ProposalDto>;

public class UpdateProposalItemCommandHandler : ICommandHandler<UpdateProposalItemCommand, ProposalDto>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProposalItemCommandHandler(IProposalRepository proposalRepository, IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProposalDto> HandleAsync(UpdateProposalItemCommand command, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId, cancellationToken);
        if (proposal == null || proposal.IsDeleted)
        {
            throw new EntityNotFoundException("Proposal", command.ProposalId);
        }

        var currency = proposal.Currency ?? "USD";
        var unitPrice = Money.Create(command.UnitPrice, currency);
        var discount = Money.Create(command.Discount, currency);

        proposal.UpdateItem(command.ItemId, command.Name, command.Description, command.Quantity, unitPrice, discount);

        _proposalRepository.Update(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return proposal.ToDto();
    }
}

public record RemoveProposalItemCommand(
    Guid ProposalId,
    Guid ItemId
) : ICommand<ProposalDto>;

public class RemoveProposalItemCommandHandler : ICommandHandler<RemoveProposalItemCommand, ProposalDto>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProposalItemCommandHandler(IProposalRepository proposalRepository, IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProposalDto> HandleAsync(RemoveProposalItemCommand command, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId, cancellationToken);
        if (proposal == null || proposal.IsDeleted)
        {
            throw new EntityNotFoundException("Proposal", command.ProposalId);
        }

        proposal.RemoveItem(command.ItemId);

        _proposalRepository.Update(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return proposal.ToDto();
    }
}

public record ChangeProposalStatusCommand(
    Guid ProposalId,
    string Status,
    string? Reason = null
) : ICommand<ProposalDto>;

public class ChangeProposalStatusCommandHandler : ICommandHandler<ChangeProposalStatusCommand, ProposalDto>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeProposalStatusCommandHandler(IProposalRepository proposalRepository, IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProposalDto> HandleAsync(ChangeProposalStatusCommand command, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId, cancellationToken);
        if (proposal == null || proposal.IsDeleted)
        {
            throw new EntityNotFoundException("Proposal", command.ProposalId);
        }

        proposal.ChangeStatus(command.Status, command.Reason);

        _proposalRepository.Update(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return proposal.ToDto();
    }
}

public record ArchiveProposalCommand(
    Guid ProposalId,
    Guid? DeletedBy = null
) : ICommand;

public class ArchiveProposalCommandHandler : ICommandHandler<ArchiveProposalCommand>
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveProposalCommandHandler(IProposalRepository proposalRepository, IUnitOfWork unitOfWork)
    {
        _proposalRepository = proposalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ArchiveProposalCommand command, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(command.ProposalId, cancellationToken);
        if (proposal == null || proposal.IsDeleted)
        {
            throw new EntityNotFoundException("Proposal", command.ProposalId);
        }

        proposal.SoftDelete(command.DeletedBy);

        _proposalRepository.Update(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
