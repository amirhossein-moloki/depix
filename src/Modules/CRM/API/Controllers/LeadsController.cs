using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.Leads.Commands;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Queries;
using ValidationException = BuildingBlocks.Common.Exceptions.ValidationException;

namespace Modules.CRM.API.Controllers;

[ApiController]
[Route("api/crm/leads")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly ICommandHandler<CreateLeadCommand, LeadDto> _createLeadHandler;
    private readonly ICommandHandler<UpdateLeadCommand, LeadDto> _updateLeadHandler;
    private readonly ICommandHandler<AssignLeadCommand, LeadDto> _assignLeadHandler;
    private readonly ICommandHandler<ChangeLeadStatusCommand, LeadDto> _changeLeadStatusHandler;
    private readonly ICommandHandler<QualifyLeadCommand, LeadDto> _qualifyLeadHandler;
    private readonly ICommandHandler<DisqualifyLeadCommand, LeadDto> _disqualifyLeadHandler;
    private readonly ICommandHandler<ArchiveLeadCommand> _archiveLeadHandler;

    private readonly IQueryHandler<GetLeadByIdQuery, LeadDto> _getLeadByIdHandler;
    private readonly IQueryHandler<GetLeadsQuery, PagedResult<LeadListItemDto>> _getLeadsHandler;
    private readonly IQueryHandler<SearchLeadsQuery, PagedResult<LeadListItemDto>> _searchLeadsHandler;
    private readonly IQueryHandler<GetLeadPipelineQuery, LeadPipelineDto> _getLeadPipelineHandler;

    private readonly IValidator<CreateLeadCommand> _createValidator;
    private readonly IValidator<UpdateLeadCommand> _updateValidator;
    private readonly IValidator<AssignLeadCommand> _assignValidator;
    private readonly IValidator<ChangeLeadStatusCommand> _changeStatusValidator;
    private readonly IValidator<QualifyLeadCommand> _qualifyValidator;
    private readonly IValidator<DisqualifyLeadCommand> _disqualifyValidator;

    public LeadsController(
        ICommandHandler<CreateLeadCommand, LeadDto> createLeadHandler,
        ICommandHandler<UpdateLeadCommand, LeadDto> updateLeadHandler,
        ICommandHandler<AssignLeadCommand, LeadDto> assignLeadHandler,
        ICommandHandler<ChangeLeadStatusCommand, LeadDto> changeLeadStatusHandler,
        ICommandHandler<QualifyLeadCommand, LeadDto> qualifyLeadHandler,
        ICommandHandler<DisqualifyLeadCommand, LeadDto> disqualifyLeadHandler,
        ICommandHandler<ArchiveLeadCommand> archiveLeadHandler,
        IQueryHandler<GetLeadByIdQuery, LeadDto> getLeadByIdHandler,
        IQueryHandler<GetLeadsQuery, PagedResult<LeadListItemDto>> getLeadsHandler,
        IQueryHandler<SearchLeadsQuery, PagedResult<LeadListItemDto>> searchLeadsHandler,
        IQueryHandler<GetLeadPipelineQuery, LeadPipelineDto> getLeadPipelineHandler,
        IValidator<CreateLeadCommand> createValidator,
        IValidator<UpdateLeadCommand> updateValidator,
        IValidator<AssignLeadCommand> assignValidator,
        IValidator<ChangeLeadStatusCommand> changeStatusValidator,
        IValidator<QualifyLeadCommand> qualifyValidator,
        IValidator<DisqualifyLeadCommand> disqualifyValidator)
    {
        _createLeadHandler = createLeadHandler;
        _updateLeadHandler = updateLeadHandler;
        _assignLeadHandler = assignLeadHandler;
        _changeLeadStatusHandler = changeLeadStatusHandler;
        _qualifyLeadHandler = qualifyLeadHandler;
        _disqualifyLeadHandler = disqualifyLeadHandler;
        _archiveLeadHandler = archiveLeadHandler;
        _getLeadByIdHandler = getLeadByIdHandler;
        _getLeadsHandler = getLeadsHandler;
        _searchLeadsHandler = searchLeadsHandler;
        _getLeadPipelineHandler = getLeadPipelineHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _assignValidator = assignValidator;
        _changeStatusValidator = changeStatusValidator;
        _qualifyValidator = qualifyValidator;
        _disqualifyValidator = disqualifyValidator;
    }

    [HttpPost]
    [Authorize(Policy = "CRM.Lead.Create")]
    public async Task<IActionResult> CreateLead([FromBody] CreateLeadRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateLeadCommand(
            request.CompanyId,
            request.Title,
            request.Source,
            request.Description,
            request.EstimatedValue,
            request.ContactId,
            request.AssignedTo,
            request.Score,
            request.Status
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Lead validation failed.", errors);
        }

        var lead = await _createLeadHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetLeadById), new { id = lead.Id }, ResponseFactory.Success(lead, "Lead created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "CRM.Lead.Read")]
    public async Task<IActionResult> GetLeads(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? assignedTo = null,
        [FromQuery] string? source = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLeadsQuery(
            page,
            pageSize,
            search,
            status,
            companyId,
            assignedTo,
            source,
            fromDate,
            toDate,
            sortBy,
            sortDescending
        );

        var result = await _getLeadsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Leads retrieved successfully."));
    }

    [HttpGet("search")]
    [Authorize(Policy = "CRM.Lead.Read")]
    public async Task<IActionResult> SearchLeads(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchLeadsQuery(search, status, companyId, page, pageSize);
        var result = await _searchLeadsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Leads search results retrieved successfully."));
    }

    [HttpGet("pipeline")]
    [Authorize(Policy = "CRM.Lead.Read")]
    public async Task<IActionResult> GetLeadPipeline(
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLeadPipelineQuery(companyId, assignedTo);
        var result = await _getLeadPipelineHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Lead pipeline retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CRM.Lead.Read")]
    public async Task<IActionResult> GetLeadById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetLeadByIdQuery(id);
        var lead = await _getLeadByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(lead, "Lead details retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CRM.Lead.Update")]
    public async Task<IActionResult> UpdateLead(Guid id, [FromBody] UpdateLeadRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateLeadCommand(
            id,
            request.Title,
            request.Source,
            request.Description,
            request.EstimatedValue,
            request.ContactId
        );

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Lead update validation failed.", errors);
        }

        var lead = await _updateLeadHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(lead, "Lead updated successfully."));
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "CRM.Lead.Assign")]
    public async Task<IActionResult> AssignLead(Guid id, [FromBody] AssignLeadRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignLeadCommand(id, request.AssignedTo);

        var validationResult = await _assignValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Lead assignment validation failed.", errors);
        }

        var lead = await _assignLeadHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(lead, "Lead assigned successfully."));
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = "CRM.Lead.StatusChange")]
    public async Task<IActionResult> ChangeLeadStatus(Guid id, [FromBody] ChangeLeadStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeLeadStatusCommand(id, request.Status, request.Reason);

        var validationResult = await _changeStatusValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Lead status change validation failed.", errors);
        }

        var lead = await _changeLeadStatusHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(lead, "Lead status changed successfully."));
    }

    [HttpPost("{id:guid}/qualify")]
    [Authorize(Policy = "CRM.Lead.Qualify")]
    public async Task<IActionResult> QualifyLead(Guid id, CancellationToken cancellationToken)
    {
        var command = new QualifyLeadCommand(id);

        var validationResult = await _qualifyValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Lead qualification validation failed.", errors);
        }

        var lead = await _qualifyLeadHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(lead, "Lead qualified successfully."));
    }

    [HttpPost("{id:guid}/disqualify")]
    [Authorize(Policy = "CRM.Lead.Qualify")]
    public async Task<IActionResult> DisqualifyLead(Guid id, [FromBody] DisqualifyLeadRequest request, CancellationToken cancellationToken)
    {
        var command = new DisqualifyLeadCommand(id, request.Reason);

        var validationResult = await _disqualifyValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Lead disqualification validation failed.", errors);
        }

        var lead = await _disqualifyLeadHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(lead, "Lead disqualified successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CRM.Lead.Delete")]
    public async Task<IActionResult> ArchiveLead(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? archivedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new ArchiveLeadCommand(id, archivedBy);
        await _archiveLeadHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Lead archived successfully.", string.Empty));
    }
}
