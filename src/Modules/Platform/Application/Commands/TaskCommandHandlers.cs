using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Application.Mappings;
using Modules.Platform.Domain.Entities;
using Modules.Platform.Domain.Repositories;

namespace Modules.Platform.Application.Commands;

public class CreateTaskCommandHandler : ICommandHandler<CreateTaskCommand, WorkTaskDto>
{
    private readonly IWorkTaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(IWorkTaskRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkTaskDto> HandleAsync(CreateTaskCommand command, CancellationToken cancellationToken = default)
    {
        var task = WorkTask.Create(
            command.AssignedTo,
            command.Title,
            command.Description,
            command.Priority,
            command.Status,
            command.Deadline);

        await _repository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }
}

public class UpdateTaskStatusCommandHandler : ICommandHandler<UpdateTaskStatusCommand, WorkTaskDto>
{
    private readonly IWorkTaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskStatusCommandHandler(IWorkTaskRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkTaskDto> HandleAsync(UpdateTaskStatusCommand command, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (task == null)
        {
            throw new EntityNotFoundException("WorkTask", command.Id);
        }

        task.UpdateStatus(command.Status);
        _repository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return task.ToDto();
    }
}

public class DeleteTaskCommandHandler : ICommandHandler<DeleteTaskCommand>
{
    private readonly IWorkTaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(IWorkTaskRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeleteTaskCommand command, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (task == null)
        {
            throw new EntityNotFoundException("WorkTask", command.Id);
        }

        task.SoftDelete(command.DeletedBy);
        _repository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
