using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Activities.DTOs;
using Modules.CRM.Application.Features.Activities.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Activities.Commands;

public record UpdateActivityCommand(
    Guid Id,
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

public class UpdateActivityCommandHandler : ICommandHandler<UpdateActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _activityRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateActivityCommandHandler(
        IActivityRepository activityRepository,
        IContactRepository contactRepository,
        IUnitOfWork unitOfWork)
    {
        _activityRepository = activityRepository;
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActivityDto> HandleAsync(UpdateActivityCommand command, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(command.Id, cancellationToken);
        if (activity == null || activity.IsDeleted)
        {
            throw new EntityNotFoundException("Activity", command.Id);
        }

        if (command.ContactId.HasValue && command.ContactId.Value != Guid.Empty)
        {
            var contact = await _contactRepository.GetByIdAsync(command.ContactId.Value, cancellationToken);
            if (contact == null)
            {
                throw new EntityNotFoundException("Contact", command.ContactId.Value);
            }
        }

        activity.UpdateInformation(
            command.Type,
            command.Subject,
            command.Description,
            command.Result,
            command.QualityScore,
            command.ContactId,
            command.CompanyId,
            command.OccurredAt,
            command.FollowUpAt,
            command.FollowUpNotes
        );

        _activityRepository.Update(activity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return activity.ToDto();
    }
}
