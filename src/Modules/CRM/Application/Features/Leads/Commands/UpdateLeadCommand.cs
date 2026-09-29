using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Mappings;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Commands;

public record UpdateLeadCommand(
    Guid Id,
    string Title,
    string Source,
    string? Description = null,
    decimal? EstimatedValue = null,
    Guid? ContactId = null
) : ICommand<LeadDto>;

public class UpdateLeadCommandHandler : ICommandHandler<UpdateLeadCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLeadCommandHandler(
        ILeadRepository leadRepository,
        IContactRepository contactRepository,
        IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> HandleAsync(UpdateLeadCommand command, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(command.Id, cancellationToken);
        if (lead == null)
        {
            throw new EntityNotFoundException("Lead", command.Id);
        }

        if (command.ContactId.HasValue && command.ContactId.Value != Guid.Empty)
        {
            var contact = await _contactRepository.GetByIdAsync(command.ContactId.Value, cancellationToken);
            if (contact == null)
            {
                throw new EntityNotFoundException("Contact", command.ContactId.Value);
            }
        }

        lead.UpdateInformation(
            command.Title,
            command.Source,
            command.Description ?? string.Empty,
            command.EstimatedValue,
            command.ContactId
        );

        _leadRepository.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return lead.ToDto();
    }
}
