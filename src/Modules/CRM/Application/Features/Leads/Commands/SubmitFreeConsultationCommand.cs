using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Domain.ValueObjects;

namespace Modules.CRM.Application.Features.Leads.Commands;

public record FreeConsultationRequest(
    string UserName,
    string PhoneNumber,
    string? Email,
    string? Description
);

public record SubmitFreeConsultationCommand(
    string UserName,
    string PhoneNumber,
    string? Email = null,
    string? Description = null
) : ICommand<LeadDto>;

public class SubmitFreeConsultationCommandHandler : ICommandHandler<SubmitFreeConsultationCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitFreeConsultationCommandHandler(
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

    public async Task<LeadDto> HandleAsync(SubmitFreeConsultationCommand command, CancellationToken cancellationToken = default)
    {
        var companyName = command.UserName;
        var company = Company.Create(
            name: companyName,
            industry: "Individual",
            website: string.Empty,
            phone: command.PhoneNumber,
            email: command.Email ?? string.Empty,
            address: new Address(string.Empty),
            type: "LEAD"
        );

        await _companyRepository.AddAsync(company, cancellationToken);

        var contact = Contact.Create(
            companyId: company.Id,
            firstName: command.UserName,
            lastName: string.Empty,
            email: command.Email ?? string.Empty,
            phone: command.PhoneNumber,
            position: "Lead",
            description: "Registered via Free Consultation form"
        );

        await _contactRepository.AddAsync(contact, cancellationToken);

        var title = $"Free Consultation Request - {command.UserName}";
        var description = string.IsNullOrWhiteSpace(command.Description)
            ? $"Free Consultation request from {command.UserName} (Phone: {command.PhoneNumber})"
            : command.Description;

        var lead = Lead.Create(
            companyId: company.Id,
            title: title,
            source: "Free Consultation",
            description: description,
            estimatedValue: null,
            contactId: contact.Id,
            assignedTo: null,
            score: 0,
            status: "NEW"
        );

        await _leadRepository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.ToDto();
    }
}
