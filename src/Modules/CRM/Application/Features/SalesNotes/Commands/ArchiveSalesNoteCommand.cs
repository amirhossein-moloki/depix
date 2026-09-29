using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.SalesNotes.Commands;

public record ArchiveSalesNoteCommand(
    Guid Id,
    Guid? ArchivedBy = null
) : ICommand;

public class ArchiveSalesNoteCommandHandler : ICommandHandler<ArchiveSalesNoteCommand>
{
    private readonly ISalesNoteRepository _salesNoteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveSalesNoteCommandHandler(
        ISalesNoteRepository salesNoteRepository,
        IUnitOfWork unitOfWork)
    {
        _salesNoteRepository = salesNoteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ArchiveSalesNoteCommand command, CancellationToken cancellationToken = default)
    {
        var salesNote = await _salesNoteRepository.GetByIdAsync(command.Id, cancellationToken);
        if (salesNote == null || salesNote.IsDeleted)
        {
            throw new EntityNotFoundException("SalesNote", command.Id);
        }

        salesNote.SoftDelete(command.ArchivedBy);
        _salesNoteRepository.Update(salesNote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
