using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Commands;

public record ChangeLeadStatusCommand(
    Guid Id,
    string Status,
    string? Reason = null
) : ICommand<LeadDto>;

public class ChangeLeadStatusCommandHandler : ICommandHandler<ChangeLeadStatusCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeLeadStatusCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> HandleAsync(ChangeLeadStatusCommand command, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(command.Id, cancellationToken);
        if (lead == null)
        {
            throw new EntityNotFoundException("Lead", command.Id);
        }

        lead.ChangeStatus(command.Status, command.Reason);

        _leadRepository.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.ToDto();
    }
}
