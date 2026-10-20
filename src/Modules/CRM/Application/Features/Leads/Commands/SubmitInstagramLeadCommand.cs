using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Domain.ValueObjects;

namespace Modules.CRM.Application.Features.Leads.Commands;

public record InstagramLeadRequest(
    string BrandName,
    string PhoneNumber,
    string InstagramId,
    string? Description
);

public record SubmitInstagramLeadCommand(
    string BrandName,
    string PhoneNumber,
    string InstagramId,
    string? Description = null
) : ICommand<LeadDto>;

public class SubmitInstagramLeadCommandHandler : ICommandHandler<SubmitInstagramLeadCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitInstagramLeadCommandHandler(
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

    public async Task<LeadDto> HandleAsync(SubmitInstagramLeadCommand command, CancellationToken cancellationToken = default)
    {
        var company = Company.Create(
            name: command.BrandName,
            industry: "Instagram Lead",
            website: $"https://instagram.com/{command.InstagramId.TrimStart('@')}",
            phone: command.PhoneNumber,
            email: string.Empty,
            address: new Address(string.Empty),
            type: "LEAD"
        );

        await _companyRepository.AddAsync(company, cancellationToken);

        var contact = Contact.Create(
            companyId: company.Id,
            firstName: command.BrandName,
            lastName: string.Empty,
            email: string.Empty,
            phone: command.PhoneNumber,
            position: "Instagram Contact",
            description: $"Instagram ID: {command.InstagramId}"
        );

        await _contactRepository.AddAsync(contact, cancellationToken);

        var title = $"Instagram Lead - {command.BrandName} ({command.InstagramId})";
        var description = string.IsNullOrWhiteSpace(command.Description)
            ? $"Instagram lead from @{command.InstagramId.TrimStart('@')} (Brand: {command.BrandName}, Phone: {command.PhoneNumber})"
            : $"Instagram ID: @{command.InstagramId.TrimStart('@')}\n{command.Description}";

        var lead = Lead.Create(
            companyId: company.Id,
            title: title,
            source: "Instagram",
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
