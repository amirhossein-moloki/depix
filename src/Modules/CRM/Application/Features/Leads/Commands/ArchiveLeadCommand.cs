using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Commands;

public record ArchiveLeadCommand(
    Guid Id,
    Guid? ArchivedBy = null
) : ICommand;

public class ArchiveLeadCommandHandler : ICommandHandler<ArchiveLeadCommand>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveLeadCommandHandler(ILeadRepository leadRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ArchiveLeadCommand command, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(command.Id, cancellationToken);
        if (lead == null)
        {
            throw new EntityNotFoundException("Lead", command.Id);
        }

        lead.SoftDelete(command.ArchivedBy);

        _leadRepository.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
