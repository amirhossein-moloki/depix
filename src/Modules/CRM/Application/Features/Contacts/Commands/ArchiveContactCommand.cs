using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Contacts.Commands;

public record ArchiveContactCommand(Guid Id, Guid? ArchivedBy = null) : ICommand;

public class ArchiveContactCommandHandler : ICommandHandler<ArchiveContactCommand>
{
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveContactCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ArchiveContactCommand command, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(command.Id, cancellationToken);
        if (contact == null)
        {
            throw new EntityNotFoundException(nameof(Contact), command.Id);
        }

        contact.SoftDelete(command.ArchivedBy);
        _contactRepository.Update(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
