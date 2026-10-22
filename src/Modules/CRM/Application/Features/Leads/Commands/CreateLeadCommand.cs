using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Commands;

public record CreateLeadCommand(
    Guid? CompanyId = null,
    string Title = "",
    string Source = "",
    string? Description = null,
    decimal? EstimatedValue = null,
    Guid? ContactId = null,
    Guid? AssignedTo = null,
    int Score = 0,
    string Status = "NEW",
    CompositeCompanyDetails? Company = null,
    CompositeContactDetails? Contact = null
) : ICommand<LeadDto>;

public class CreateLeadCommandHandler : ICommandHandler<CreateLeadCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(
        ILeadRepository leadRepository,
        ICompanyRepository companyRepository,
        IContactRepository contactRepository,
        IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _companyRepository = companyRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> HandleAsync(CreateLeadCommand command, CancellationToken cancellationToken = default)
    {
        Company? company = null;
        Guid companyId;

        if (command.CompanyId.HasValue && command.CompanyId.Value != Guid.Empty)
        {
            company = await _companyRepository.GetByIdAsync(command.CompanyId.Value, cancellationToken);
            if (company == null)
            {
                throw new EntityNotFoundException("Company", command.CompanyId.Value);
            }
            companyId = company.Id;
        }
        else if (command.Company != null)
        {
            var address = new Domain.ValueObjects.Address(command.Company.AddressText ?? string.Empty);
            company = Company.Create(
                command.Company.Name,
                command.Company.Industry ?? string.Empty,
                command.Company.Website ?? string.Empty,
                command.Company.Phone ?? string.Empty,
                command.Company.Email ?? string.Empty,
                address,
                string.IsNullOrWhiteSpace(command.Company.Type) ? "LEAD" : command.Company.Type
            );
            await _companyRepository.AddAsync(company, cancellationToken);
            companyId = company.Id;
        }
        else
        {
            throw new ValidationException("Either CompanyId or Company details must be provided.");
        }

        Contact? contact = null;
        Guid? contactId = command.ContactId;

        if (command.ContactId.HasValue && command.ContactId.Value != Guid.Empty)
        {
            contact = await _contactRepository.GetByIdAsync(command.ContactId.Value, cancellationToken);
            if (contact == null)
            {
                throw new EntityNotFoundException("Contact", command.ContactId.Value);
            }
        }
        else if (command.Contact != null)
        {
            contact = Contact.Create(
                companyId,
                command.Contact.FirstName,
                command.Contact.LastName,
                command.Contact.Email ?? string.Empty,
                command.Contact.Phone ?? string.Empty,
                command.Contact.Position ?? string.Empty,
                command.Contact.Description ?? string.Empty,
                command.Contact.IsDecisionMaker,
                command.Contact.InfluenceLevel ?? string.Empty
            );
            await _contactRepository.AddAsync(contact, cancellationToken);
            contactId = contact.Id;
        }

        var lead = Lead.Create(
            companyId,
            command.Title,
            command.Source,
            command.Description ?? string.Empty,
            command.EstimatedValue,
            contactId,
            command.AssignedTo,
            command.Score,
            command.Status
        );

        await _leadRepository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdLead = await _leadRepository.GetByIdAsync(lead.Id, cancellationToken);
        return (createdLead ?? lead).ToDto();
    }
}
