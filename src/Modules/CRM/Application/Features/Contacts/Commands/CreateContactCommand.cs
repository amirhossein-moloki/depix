using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Contacts.DTOs;
using Modules.CRM.Application.Features.Contacts.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Contacts.Commands;

public record CreateContactCommand(
    Guid CompanyId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Position,
    string Description,
    bool IsDecisionMaker = false,
    string InfluenceLevel = ""
) : ICommand<ContactDto>;

public class CreateContactCommandHandler : ICommandHandler<CreateContactCommand, ContactDto>
{
    private readonly IContactRepository _contactRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateContactCommandHandler(
        IContactRepository contactRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ContactDto> HandleAsync(CreateContactCommand command, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(command.CompanyId, cancellationToken);
        if (company == null)
        {
            throw new EntityNotFoundException(nameof(Company), command.CompanyId);
        }

        var contact = Contact.Create(
            command.CompanyId,
            command.FirstName,
            command.LastName,
            command.Email,
            command.Phone,
            command.Position,
            command.Description,
            command.IsDecisionMaker,
            command.InfluenceLevel
        );

        await _contactRepository.AddAsync(contact, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return contact.ToDto();
    }
}
