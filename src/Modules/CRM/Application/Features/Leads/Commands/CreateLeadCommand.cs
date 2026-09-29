using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Commands;

public record CreateLeadCommand(
    Guid CompanyId,
    string Title,
    string Source,
    string? Description = null,
    decimal? EstimatedValue = null,
    Guid? ContactId = null,
    Guid? AssignedTo = null,
    int Score = 0,
    string Status = "NEW"
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
        var company = await _companyRepository.GetByIdAsync(command.CompanyId, cancellationToken);
        if (company == null)
        {
            throw new EntityNotFoundException("Company", command.CompanyId);
        }

        if (command.ContactId.HasValue && command.ContactId.Value != Guid.Empty)
        {
            var contact = await _contactRepository.GetByIdAsync(command.ContactId.Value, cancellationToken);
            if (contact == null)
            {
                throw new EntityNotFoundException("Contact", command.ContactId.Value);
            }
        }

        var lead = Lead.Create(
            command.CompanyId,
            command.Title,
            command.Source,
            command.Description ?? string.Empty,
            command.EstimatedValue,
            command.ContactId,
            command.AssignedTo,
            command.Score,
            command.Status
        );

        await _leadRepository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.ToDto();
    }
}
