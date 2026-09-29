using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.SalesNotes.DTOs;
using Modules.CRM.Application.Features.SalesNotes.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Events;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.SalesNotes.Commands;

public record CreateSalesNoteCommand(
    Guid LeadId,
    string Title,
    string NeedAnalysis,
    string Objections,
    string Strategy,
    int Probability,
    Guid CreatedBy,
    Guid? CompanyId = null,
    Guid? ContactId = null,
    string? CompetitorsMentioned = null,
    string? BudgetInformation = null,
    string? DecisionMakerInfo = null
) : ICommand<SalesNoteDto>;

public class CreateSalesNoteCommandHandler : ICommandHandler<CreateSalesNoteCommand, SalesNoteDto>
{
    private readonly ISalesNoteRepository _salesNoteRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSalesNoteCommandHandler(
        ISalesNoteRepository salesNoteRepository,
        ILeadRepository leadRepository,
        IContactRepository contactRepository,
        IUnitOfWork unitOfWork)
    {
        _salesNoteRepository = salesNoteRepository;
        _leadRepository = leadRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesNoteDto> HandleAsync(CreateSalesNoteCommand command, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(command.LeadId, cancellationToken);
        if (lead == null)
        {
            throw new EntityNotFoundException("Lead", command.LeadId);
        }

        Guid? companyId = command.CompanyId ?? lead.CompanyId;
        Guid? contactId = command.ContactId ?? lead.ContactId;

        if (contactId.HasValue && contactId.Value != Guid.Empty)
        {
            var contact = await _contactRepository.GetByIdAsync(contactId.Value, cancellationToken);
            if (contact == null)
            {
                throw new EntityNotFoundException("Contact", contactId.Value);
            }
        }

        var salesNote = SalesNote.Create(
            command.LeadId,
            command.Title,
            command.NeedAnalysis,
            command.Objections,
            command.Strategy,
            command.Probability,
            command.CreatedBy,
            companyId,
            contactId,
            command.CompetitorsMentioned,
            command.BudgetInformation,
            command.DecisionMakerInfo
        );

        lead.AddSalesNote(salesNote);
        lead.AddDomainEvent(new SalesNoteCreatedEvent(salesNote.Id, lead.Id, command.CreatedBy));

        await _salesNoteRepository.AddAsync(salesNote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return salesNote.ToDto();
    }
}
