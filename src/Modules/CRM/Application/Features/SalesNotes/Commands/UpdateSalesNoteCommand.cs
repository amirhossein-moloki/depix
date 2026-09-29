using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.SalesNotes.DTOs;
using Modules.CRM.Application.Features.SalesNotes.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.SalesNotes.Commands;

public record UpdateSalesNoteCommand(
    Guid Id,
    string Title,
    string NeedAnalysis,
    string Objections,
    string Strategy,
    int Probability,
    Guid? CompanyId = null,
    Guid? ContactId = null,
    string? CompetitorsMentioned = null,
    string? BudgetInformation = null,
    string? DecisionMakerInfo = null
) : ICommand<SalesNoteDto>;

public class UpdateSalesNoteCommandHandler : ICommandHandler<UpdateSalesNoteCommand, SalesNoteDto>
{
    private readonly ISalesNoteRepository _salesNoteRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSalesNoteCommandHandler(
        ISalesNoteRepository salesNoteRepository,
        IContactRepository contactRepository,
        IUnitOfWork unitOfWork)
    {
        _salesNoteRepository = salesNoteRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesNoteDto> HandleAsync(UpdateSalesNoteCommand command, CancellationToken cancellationToken = default)
    {
        var salesNote = await _salesNoteRepository.GetByIdAsync(command.Id, cancellationToken);
        if (salesNote == null || salesNote.IsDeleted)
        {
            throw new EntityNotFoundException("SalesNote", command.Id);
        }

        if (command.ContactId.HasValue && command.ContactId.Value != Guid.Empty)
        {
            var contact = await _contactRepository.GetByIdAsync(command.ContactId.Value, cancellationToken);
            if (contact == null)
            {
                throw new EntityNotFoundException("Contact", command.ContactId.Value);
            }
        }

        salesNote.UpdateInformation(
            command.Title,
            command.NeedAnalysis,
            command.Objections,
            command.Strategy,
            command.Probability,
            command.CompanyId,
            command.ContactId,
            command.CompetitorsMentioned,
            command.BudgetInformation,
            command.DecisionMakerInfo
        );

        _salesNoteRepository.Update(salesNote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return salesNote.ToDto();
    }
}
