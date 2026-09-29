using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Activities.DTOs;
using Modules.CRM.Application.Features.Activities.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Events;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Activities.Commands;

public record CreateActivityCommand(
    Guid LeadId,
    Guid? UserId,
    string Type,
    string Subject,
    string Description,
    string Result,
    int QualityScore = 0,
    Guid? ContactId = null,
    Guid? CompanyId = null,
    DateTime? OccurredAt = null,
    DateTime? FollowUpAt = null,
    string? FollowUpNotes = null
) : ICommand<ActivityDto>;

public class CreateActivityCommandHandler : ICommandHandler<CreateActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activityRepository;
    private readonly ILeadRepository _leadRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateActivityCommandHandler(
        IActivityRepository activityRepository,
        ILeadRepository leadRepository,
        IContactRepository contactRepository,
        IUnitOfWork unitOfWork)
    {
        _activityRepository = activityRepository;
        _leadRepository = leadRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActivityDto> HandleAsync(CreateActivityCommand command, CancellationToken cancellationToken = default)
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

        var activity = Activity.Create(
            command.LeadId,
            command.UserId ?? lead.AssignedTo,
            command.Type,
            command.Subject,
            command.Description,
            command.Result,
            command.QualityScore,
            contactId,
            companyId,
            command.OccurredAt,
            command.FollowUpAt,
            command.FollowUpNotes
        );

        lead.AddActivity(activity);
        lead.AddDomainEvent(new ActivityCreatedEvent(activity.Id, lead.Id, activity.Type, activity.UserId));

        await _activityRepository.AddAsync(activity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return activity.ToDto();
    }
}
