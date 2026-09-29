using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.CRM.Application.Features.Activities.Commands;
using Modules.CRM.Application.Features.Activities.DTOs;
using Modules.CRM.Application.Features.Activities.Queries;
using Modules.CRM.Application.Features.Companies.DTOs;
using ValidationException = BuildingBlocks.Common.Exceptions.ValidationException;

namespace Modules.CRM.API.Controllers;

/// <summary>
/// API Controller managing CRM interaction activities and history.
/// </summary>
[ApiController]
[Route("api/crm/activities")]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly ICommandHandler<CreateActivityCommand, ActivityDto> _createActivityHandler;
    private readonly ICommandHandler<UpdateActivityCommand, ActivityDto> _updateActivityHandler;
    private readonly ICommandHandler<ArchiveActivityCommand> _archiveActivityHandler;
    private readonly IQueryHandler<GetActivityByIdQuery, ActivityDto> _getActivityByIdHandler;
    private readonly IQueryHandler<GetActivitiesQuery, PagedResult<ActivityListItemDto>> _getActivitiesHandler;
    private readonly IQueryHandler<GetLeadActivitiesQuery, LeadActivityHistoryDto> _getLeadActivitiesHandler;
    private readonly IValidator<CreateActivityCommand> _createValidator;
    private readonly IValidator<UpdateActivityCommand> _updateValidator;

    public ActivitiesController(
        ICommandHandler<CreateActivityCommand, ActivityDto> createActivityHandler,
        ICommandHandler<UpdateActivityCommand, ActivityDto> updateActivityHandler,
        ICommandHandler<ArchiveActivityCommand> archiveActivityHandler,
        IQueryHandler<GetActivityByIdQuery, ActivityDto> getActivityByIdHandler,
        IQueryHandler<GetActivitiesQuery, PagedResult<ActivityListItemDto>> getActivitiesHandler,
        IQueryHandler<GetLeadActivitiesQuery, LeadActivityHistoryDto> getLeadActivitiesHandler,
        IValidator<CreateActivityCommand> createValidator,
        IValidator<UpdateActivityCommand> updateValidator)
    {
        _createActivityHandler = createActivityHandler;
        _updateActivityHandler = updateActivityHandler;
        _archiveActivityHandler = archiveActivityHandler;
        _getActivityByIdHandler = getActivityByIdHandler;
        _getActivitiesHandler = getActivitiesHandler;
        _getLeadActivitiesHandler = getLeadActivitiesHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    [Authorize(Policy = "CRM.Activity.Create")]
    public async Task<IActionResult> CreateActivity([FromBody] CreateActivityRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? currentUserId = Guid.TryParse(userIdClaim, out var uId) ? uId : null;

        var command = new CreateActivityCommand(
            request.LeadId,
            request.UserId ?? currentUserId,
            request.Type,
            request.Subject,
            request.Description,
            request.Result,
            request.QualityScore,
            request.ContactId,
            request.CompanyId,
            request.OccurredAt,
            request.FollowUpAt,
            request.FollowUpNotes
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Activity validation failed.", errors);
        }

        var activity = await _createActivityHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetActivityById), new { id = activity.Id }, ResponseFactory.Success(activity, "Activity created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "CRM.Activity.Read")]
    public async Task<IActionResult> GetActivities(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? leadId = null,
        [FromQuery] Guid? contactId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? type = null,
        [FromQuery] string? result = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetActivitiesQuery(page, pageSize, leadId, contactId, companyId, userId, type, result, fromDate, toDate, search);
        var res = await _getActivitiesHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(res, "Activities retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CRM.Activity.Read")]
    public async Task<IActionResult> GetActivityById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetActivityByIdQuery(id);
        var activity = await _getActivityByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(activity, "Activity details retrieved successfully."));
    }

    [HttpGet("/api/crm/leads/{leadId:guid}/activities")]
    [Authorize(Policy = "CRM.Activity.Read")]
    public async Task<IActionResult> GetLeadActivities(Guid leadId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetLeadActivitiesQuery(leadId, page, pageSize);
        var result = await _getLeadActivitiesHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Lead activities retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CRM.Activity.Update")]
    public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] UpdateActivityRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateActivityCommand(
            id,
            request.Type,
            request.Subject,
            request.Description,
            request.Result,
            request.QualityScore,
            request.ContactId,
            request.CompanyId,
            request.OccurredAt,
            request.FollowUpAt,
            request.FollowUpNotes
        );

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Activity update validation failed.", errors);
        }

        var activity = await _updateActivityHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(activity, "Activity updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CRM.Activity.Delete")]
    public async Task<IActionResult> ArchiveActivity(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? archivedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new ArchiveActivityCommand(id, archivedBy);
        await _archiveActivityHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Activity archived successfully.", string.Empty));
    }
}
