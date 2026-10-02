using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Platform.Application.DTOs;

namespace Modules.Platform.API.Controllers;

[ApiController]
[Route("api/platform/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ICommandHandler<CreateTaskCommand, WorkTaskDto> _createTaskHandler;
    private readonly ICommandHandler<UpdateTaskStatusCommand, WorkTaskDto> _updateStatusHandler;
    private readonly ICommandHandler<DeleteTaskCommand> _deleteTaskHandler;
    private readonly IQueryHandler<GetTaskByIdQuery, WorkTaskDto> _getTaskByIdHandler;
    private readonly IQueryHandler<GetTasksQuery, List<WorkTaskDto>> _getTasksHandler;
    private readonly IValidator<CreateTaskCommand> _createValidator;

    public TasksController(
        ICommandHandler<CreateTaskCommand, WorkTaskDto> createTaskHandler,
        ICommandHandler<UpdateTaskStatusCommand, WorkTaskDto> updateStatusHandler,
        ICommandHandler<DeleteTaskCommand> deleteTaskHandler,
        IQueryHandler<GetTaskByIdQuery, WorkTaskDto> getTaskByIdHandler,
        IQueryHandler<GetTasksQuery, List<WorkTaskDto>> getTasksHandler,
        IValidator<CreateTaskCommand> createValidator)
    {
        _createTaskHandler = createTaskHandler;
        _updateStatusHandler = updateStatusHandler;
        _deleteTaskHandler = deleteTaskHandler;
        _getTaskByIdHandler = getTaskByIdHandler;
        _getTasksHandler = getTasksHandler;
        _createValidator = createValidator;
    }

    [HttpPost]
    [Authorize(Policy = "Platform.Task.Create")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand(
            request.AssignedTo,
            request.Title,
            request.Description,
            request.Priority,
            request.Status,
            request.Deadline);

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Task creation validation failed.", errors);
        }

        var task = await _createTaskHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, ResponseFactory.Success(task, "Task created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "Platform.Task.Read")]
    public async Task<IActionResult> GetTasks(
        [FromQuery] Guid? assignedTo = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTasksQuery(assignedTo, status);
        var tasks = await _getTasksHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(tasks, "Tasks retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Platform.Task.Read")]
    public async Task<IActionResult> GetTaskById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetTaskByIdQuery(id);
        var task = await _getTaskByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(task, "Task retrieved successfully."));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "Platform.Task.Update")]
    public async Task<IActionResult> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskStatusCommand(id, request.Status);
        var task = await _updateStatusHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(task, "Task status updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Platform.Task.Delete")]
    public async Task<IActionResult> DeleteTask(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? deletedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new DeleteTaskCommand(id, deletedBy);
        await _deleteTaskHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Task deleted successfully.", string.Empty));
    }
}
