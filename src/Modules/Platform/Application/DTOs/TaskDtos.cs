using BuildingBlocks.Application.CQRS;

namespace Modules.Platform.Application.DTOs;

public record WorkTaskDto(
    Guid Id,
    Guid? AssignedTo,
    string Title,
    string Description,
    string Priority,
    string Status,
    DateOnly? Deadline,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateTaskRequest(
    Guid? AssignedTo,
    string Title,
    string Description,
    string Priority = "MEDIUM",
    string Status = "TODO",
    DateOnly? Deadline = null);

public record UpdateTaskStatusRequest(
    string Status);

public record CreateTaskCommand(
    Guid? AssignedTo,
    string Title,
    string Description,
    string Priority = "MEDIUM",
    string Status = "TODO",
    DateOnly? Deadline = null) : ICommand<WorkTaskDto>;

public record UpdateTaskStatusCommand(
    Guid Id,
    string Status) : ICommand<WorkTaskDto>;

public record DeleteTaskCommand(
    Guid Id,
    Guid? DeletedBy = null) : ICommand;

public record GetTaskByIdQuery(
    Guid Id) : IQuery<WorkTaskDto>;

public record GetTasksQuery(
    Guid? AssignedTo = null,
    string? Status = null) : IQuery<List<WorkTaskDto>>;
