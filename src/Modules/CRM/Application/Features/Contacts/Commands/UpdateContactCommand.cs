using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Contacts.DTOs;
using Modules.CRM.Application.Features.Contacts.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Contacts.Commands;

public record UpdateContactCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Position,
    string Description,
    bool IsDecisionMaker = false,
    string InfluenceLevel = ""
) : ICommand<ContactDto>;

public class UpdateContactCommandHandler : ICommandHandler<UpdateContactCommand, ContactDto>
{
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateContactCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ContactDto> HandleAsync(UpdateContactCommand command, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(command.Id, cancellationToken);
        if (contact == null)
        {
            throw new EntityNotFoundException(nameof(Contact), command.Id);
        }

        contact.UpdateInformation(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone,
            command.Position,
            command.Description,
            command.IsDecisionMaker,
            command.InfluenceLevel
        );

        _contactRepository.Update(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return contact.ToDto();
    }
}
