using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Activities.Commands;

public record ArchiveActivityCommand(
    Guid Id,
    Guid? ArchivedBy = null
) : ICommand;

public class ArchiveActivityCommandHandler : ICommandHandler<ArchiveActivityCommand>
{
    private readonly IActivityRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveActivityCommandHandler(
        IActivityRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ArchiveActivityCommand command, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(command.Id, cancellationToken);
        if (activity == null || activity.IsDeleted)
        {
            throw new EntityNotFoundException("Activity", command.Id);
        }

        activity.SoftDelete(command.ArchivedBy);
        _activityRepository.Update(activity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
