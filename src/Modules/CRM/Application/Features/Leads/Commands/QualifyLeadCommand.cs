using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Commands;

public record QualifyLeadCommand(
    Guid Id
) : ICommand<LeadDto>;

public class QualifyLeadCommandHandler : ICommandHandler<QualifyLeadCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QualifyLeadCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> HandleAsync(QualifyLeadCommand command, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(command.Id, cancellationToken);
        if (lead == null)
        {
            throw new EntityNotFoundException("Lead", command.Id);
        }

        lead.Qualify();

        _leadRepository.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.ToDto();
    }
}
