using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Support.Application.Commands;
using Modules.Support.Application.DTOs;
using Modules.Support.Application.Queries;
using Modules.Support.Domain.Repositories;

namespace Modules.Support.API.Controllers;

[ApiController]
[Route("api/support")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ICommandHandler<CreateTicketCommand, TicketDetailDto> _createTicketHandler;
    private readonly ICommandHandler<UpdateTicketCommand, TicketDetailDto> _updateTicketHandler;
    private readonly ICommandHandler<AssignTicketCommand, TicketDetailDto> _assignTicketHandler;
    private readonly ICommandHandler<ChangeTicketPriorityCommand, TicketDetailDto> _changePriorityHandler;
    private readonly ICommandHandler<ChangeTicketStatusCommand, TicketDetailDto> _changeStatusHandler;
    private readonly ICommandHandler<ResolveTicketCommand, TicketDetailDto> _resolveTicketHandler;
    private readonly ICommandHandler<CloseTicketCommand, TicketDetailDto> _closeTicketHandler;
    private readonly ICommandHandler<CancelTicketCommand, TicketDetailDto> _cancelTicketHandler;
    private readonly ICommandHandler<ReopenTicketCommand, TicketDetailDto> _reopenTicketHandler;
    private readonly ICommandHandler<DeleteTicketCommand, bool> _deleteTicketHandler;
    private readonly ICommandHandler<AddTicketCommentCommand, TicketCommentDto> _addCommentHandler;

    private readonly IQueryHandler<GetTicketByIdQuery, TicketDetailDto> _getTicketByIdHandler;
    private readonly IQueryHandler<GetTicketsQuery, PagedResult<TicketListItemDto>> _getTicketsHandler;
    private readonly IQueryHandler<GetCustomerTicketsQuery, List<TicketListItemDto>> _getCustomerTicketsHandler;
    private readonly IQueryHandler<GetProjectTicketsQuery, List<TicketListItemDto>> _getProjectTicketsHandler;
    private readonly IQueryHandler<GetTicketCommentsQuery, List<TicketCommentDto>> _getTicketCommentsHandler;

    public TicketsController(
        ICommandHandler<CreateTicketCommand, TicketDetailDto> createTicketHandler,
        ICommandHandler<UpdateTicketCommand, TicketDetailDto> updateTicketHandler,
        ICommandHandler<AssignTicketCommand, TicketDetailDto> assignTicketHandler,
        ICommandHandler<ChangeTicketPriorityCommand, TicketDetailDto> changePriorityHandler,
        ICommandHandler<ChangeTicketStatusCommand, TicketDetailDto> changeStatusHandler,
        ICommandHandler<ResolveTicketCommand, TicketDetailDto> resolveTicketHandler,
        ICommandHandler<CloseTicketCommand, TicketDetailDto> closeTicketHandler,
        ICommandHandler<CancelTicketCommand, TicketDetailDto> cancelTicketHandler,
        ICommandHandler<ReopenTicketCommand, TicketDetailDto> reopenTicketHandler,
        ICommandHandler<DeleteTicketCommand, bool> deleteTicketHandler,
        ICommandHandler<AddTicketCommentCommand, TicketCommentDto> addCommentHandler,
        IQueryHandler<GetTicketByIdQuery, TicketDetailDto> getTicketByIdHandler,
        IQueryHandler<GetTicketsQuery, PagedResult<TicketListItemDto>> getTicketsHandler,
        IQueryHandler<GetCustomerTicketsQuery, List<TicketListItemDto>> getCustomerTicketsHandler,
        IQueryHandler<GetProjectTicketsQuery, List<TicketListItemDto>> getProjectTicketsHandler,
        IQueryHandler<GetTicketCommentsQuery, List<TicketCommentDto>> getTicketCommentsHandler)
    {
        _createTicketHandler = createTicketHandler;
        _updateTicketHandler = updateTicketHandler;
        _assignTicketHandler = assignTicketHandler;
        _changePriorityHandler = changePriorityHandler;
        _changeStatusHandler = changeStatusHandler;
        _resolveTicketHandler = resolveTicketHandler;
        _closeTicketHandler = closeTicketHandler;
        _cancelTicketHandler = cancelTicketHandler;
        _reopenTicketHandler = reopenTicketHandler;
        _deleteTicketHandler = deleteTicketHandler;
        _addCommentHandler = addCommentHandler;
        _getTicketByIdHandler = getTicketByIdHandler;
        _getTicketsHandler = getTicketsHandler;
        _getCustomerTicketsHandler = getCustomerTicketsHandler;
        _getProjectTicketsHandler = getProjectTicketsHandler;
        _getTicketCommentsHandler = getTicketCommentsHandler;
    }

    [HttpPost("tickets")]
    [Authorize(Policy = "Support.Ticket.Create")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTicket(
        [FromBody] CreateTicketRequest request,
        [FromServices] IValidator<CreateTicketCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new CreateTicketCommand(
            request.CustomerId,
            request.ProjectId,
            request.ContactId,
            request.Subject,
            request.Description,
            request.Priority,
            request.Category,
            request.AssignedToUserId,
            request.DueAt);

        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _createTicketHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetTicketById), new { id = result.Id }, ResponseFactory.Success(result, "Support ticket created successfully."));
    }

    [HttpGet("tickets")]
    [Authorize(Policy = "Support.Ticket.Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TicketListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTickets(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? contactId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? category = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? unresolvedOnly = null,
        [FromQuery] DateTime? openedFrom = null,
        [FromQuery] DateTime? openedTo = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new TicketFilterParams(
            customerId,
            projectId,
            contactId,
            status,
            priority,
            category,
            assignedToUserId,
            searchTerm,
            unresolvedOnly,
            openedFrom,
            openedTo,
            pageNumber,
            pageSize);

        var query = new GetTicketsQuery(filter);
        var result = await _getTicketsHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support tickets retrieved successfully."));
    }

    [HttpGet("tickets/{id:guid}")]
    [Authorize(Policy = "Support.Ticket.Read")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTicketById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTicketByIdQuery(id);
        var result = await _getTicketByIdHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket retrieved successfully."));
    }

    [HttpPut("tickets/{id:guid}")]
    [Authorize(Policy = "Support.Ticket.Update")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTicket(
        [FromRoute] Guid id,
        [FromBody] UpdateTicketRequest request,
        [FromServices] IValidator<UpdateTicketCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTicketCommand(
            id,
            request.Subject,
            request.Description,
            request.Category,
            request.ProjectId,
            request.ContactId,
            request.DueAt);

        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _updateTicketHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket updated successfully."));
    }

    [HttpDelete("tickets/{id:guid}")]
    [Authorize(Policy = "Support.Ticket.Delete")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTicket(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTicketCommand(id);
        var result = await _deleteTicketHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket deleted successfully."));
    }

    [HttpPost("tickets/{id:guid}/assign")]
    [Authorize(Policy = "Support.Ticket.Assign")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignTicket(
        [FromRoute] Guid id,
        [FromBody] AssignTicketRequest request,
        [FromServices] IValidator<AssignTicketCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new AssignTicketCommand(id, request.AssignedToUserId, User?.Identity?.Name);
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _assignTicketHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket assigned successfully."));
    }

    [HttpPost("tickets/{id:guid}/priority")]
    [Authorize(Policy = "Support.Ticket.StatusChange")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangePriority(
        [FromRoute] Guid id,
        [FromBody] ChangeTicketPriorityRequest request,
        [FromServices] IValidator<ChangeTicketPriorityCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new ChangeTicketPriorityCommand(id, request.Priority, User?.Identity?.Name);
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _changePriorityHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket priority changed successfully."));
    }

    [HttpPost("tickets/{id:guid}/status")]
    [Authorize(Policy = "Support.Ticket.StatusChange")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeTicketStatusRequest request,
        [FromServices] IValidator<ChangeTicketStatusCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new ChangeTicketStatusCommand(id, request.Status, User?.Identity?.Name);
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _changeStatusHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket status changed successfully."));
    }

    [HttpPost("tickets/{id:guid}/resolve")]
    [Authorize(Policy = "Support.Ticket.Resolve")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResolveTicket(
        [FromRoute] Guid id,
        [FromBody] ResolveTicketRequest request,
        [FromServices] IValidator<ResolveTicketCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new ResolveTicketCommand(id, request.Resolution, request.ResolvedAt, User?.Identity?.Name);
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _resolveTicketHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket resolved successfully."));
    }

    [HttpPost("tickets/{id:guid}/close")]
    [Authorize(Policy = "Support.Ticket.Close")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseTicket(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CloseTicketCommand(id, User?.Identity?.Name);
        var result = await _closeTicketHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket closed successfully."));
    }

    [HttpPost("tickets/{id:guid}/cancel")]
    [Authorize(Policy = "Support.Ticket.Close")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelTicket(
        [FromRoute] Guid id,
        [FromBody] CancelTicketRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CancelTicketCommand(id, request.Reason, User?.Identity?.Name);
        var result = await _cancelTicketHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket cancelled successfully."));
    }

    [HttpPost("tickets/{id:guid}/reopen")]
    [Authorize(Policy = "Support.Ticket.Update")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReopenTicket(
        [FromRoute] Guid id,
        [FromBody] ReopenTicketRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReopenTicketCommand(id, request.Reason, User?.Identity?.Name);
        var result = await _reopenTicketHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Support ticket reopened successfully."));
    }

    [HttpGet("customers/{customerId:guid}/tickets")]
    [Authorize(Policy = "Support.Ticket.Read")]
    [ProducesResponseType(typeof(ApiResponse<List<TicketListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerTickets(
        [FromRoute] Guid customerId,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerTicketsQuery(customerId);
        var result = await _getCustomerTicketsHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Customer support tickets retrieved successfully."));
    }

    [HttpGet("projects/{projectId:guid}/tickets")]
    [Authorize(Policy = "Support.Ticket.Read")]
    [ProducesResponseType(typeof(ApiResponse<List<TicketListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectTickets(
        [FromRoute] Guid projectId,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectTicketsQuery(projectId);
        var result = await _getProjectTicketsHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Project support tickets retrieved successfully."));
    }

    [HttpPost("tickets/{ticketId:guid}/comments")]
    [Authorize(Policy = "Support.Comment.Create")]
    [ProducesResponseType(typeof(ApiResponse<TicketCommentDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddComment(
        [FromRoute] Guid ticketId,
        [FromBody] AddTicketCommentRequest request,
        [FromServices] IValidator<AddTicketCommentCommand> validator,
        CancellationToken cancellationToken)
    {
        var authorName = User?.Identity?.Name ?? "User";
        var command = new AddTicketCommentCommand(ticketId, null, authorName, request.Message);
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _addCommentHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Ticket comment added successfully."));
    }

    [HttpGet("tickets/{ticketId:guid}/comments")]
    [Authorize(Policy = "Support.Comment.Read")]
    [ProducesResponseType(typeof(ApiResponse<List<TicketCommentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetComments(
        [FromRoute] Guid ticketId,
        CancellationToken cancellationToken)
    {
        var query = new GetTicketCommentsQuery(ticketId);
        var result = await _getTicketCommentsHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Ticket comments retrieved successfully."));
    }
}
