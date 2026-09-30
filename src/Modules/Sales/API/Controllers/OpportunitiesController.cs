using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Sales.Application.Features.Opportunities.Commands;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Queries;
using ValidationException = BuildingBlocks.Common.Exceptions.ValidationException;

namespace Modules.Sales.API.Controllers;

[ApiController]
[Route("api/sales/opportunities")]
[Authorize]
public class OpportunitiesController : ControllerBase
{
    private readonly ICommandHandler<CreateOpportunityCommand, OpportunityDto> _createOpportunityHandler;
    private readonly ICommandHandler<UpdateOpportunityCommand, OpportunityDto> _updateOpportunityHandler;
    private readonly ICommandHandler<AssignOpportunityCommand, OpportunityDto> _assignOpportunityHandler;
    private readonly ICommandHandler<ChangeOpportunityStageCommand, OpportunityDto> _changeStageHandler;
    private readonly ICommandHandler<MarkOpportunityWonCommand, OpportunityDto> _markWonHandler;
    private readonly ICommandHandler<MarkOpportunityLostCommand, OpportunityDto> _markLostHandler;
    private readonly ICommandHandler<ArchiveOpportunityCommand> _archiveOpportunityHandler;

    private readonly IQueryHandler<GetOpportunityByIdQuery, OpportunityDto> _getByIdHandler;
    private readonly IQueryHandler<GetOpportunitiesQuery, PagedResult<OpportunityListItemDto>> _getOpportunitiesHandler;
    private readonly IQueryHandler<SearchOpportunitiesQuery, PagedResult<OpportunityListItemDto>> _searchOpportunitiesHandler;
    private readonly IQueryHandler<GetOpportunityPipelineQuery, OpportunityPipelineDto> _getPipelineHandler;

    private readonly IValidator<CreateOpportunityCommand> _createValidator;
    private readonly IValidator<UpdateOpportunityCommand> _updateValidator;
    private readonly IValidator<AssignOpportunityCommand> _assignValidator;
    private readonly IValidator<ChangeOpportunityStageCommand> _changeStageValidator;
    private readonly IValidator<MarkOpportunityWonCommand> _markWonValidator;
    private readonly IValidator<MarkOpportunityLostCommand> _markLostValidator;

    public OpportunitiesController(
        ICommandHandler<CreateOpportunityCommand, OpportunityDto> createOpportunityHandler,
        ICommandHandler<UpdateOpportunityCommand, OpportunityDto> updateOpportunityHandler,
        ICommandHandler<AssignOpportunityCommand, OpportunityDto> assignOpportunityHandler,
        ICommandHandler<ChangeOpportunityStageCommand, OpportunityDto> changeStageHandler,
        ICommandHandler<MarkOpportunityWonCommand, OpportunityDto> markWonHandler,
        ICommandHandler<MarkOpportunityLostCommand, OpportunityDto> markLostHandler,
        ICommandHandler<ArchiveOpportunityCommand> archiveOpportunityHandler,
        IQueryHandler<GetOpportunityByIdQuery, OpportunityDto> getByIdHandler,
        IQueryHandler<GetOpportunitiesQuery, PagedResult<OpportunityListItemDto>> getOpportunitiesHandler,
        IQueryHandler<SearchOpportunitiesQuery, PagedResult<OpportunityListItemDto>> searchOpportunitiesHandler,
        IQueryHandler<GetOpportunityPipelineQuery, OpportunityPipelineDto> getPipelineHandler,
        IValidator<CreateOpportunityCommand> createValidator,
        IValidator<UpdateOpportunityCommand> updateValidator,
        IValidator<AssignOpportunityCommand> assignValidator,
        IValidator<ChangeOpportunityStageCommand> changeStageValidator,
        IValidator<MarkOpportunityWonCommand> markWonValidator,
        IValidator<MarkOpportunityLostCommand> markLostValidator)
    {
        _createOpportunityHandler = createOpportunityHandler;
        _updateOpportunityHandler = updateOpportunityHandler;
        _assignOpportunityHandler = assignOpportunityHandler;
        _changeStageHandler = changeStageHandler;
        _markWonHandler = markWonHandler;
        _markLostHandler = markLostHandler;
        _archiveOpportunityHandler = archiveOpportunityHandler;
        _getByIdHandler = getByIdHandler;
        _getOpportunitiesHandler = getOpportunitiesHandler;
        _searchOpportunitiesHandler = searchOpportunitiesHandler;
        _getPipelineHandler = getPipelineHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _assignValidator = assignValidator;
        _changeStageValidator = changeStageValidator;
        _markWonValidator = markWonValidator;
        _markLostValidator = markLostValidator;
    }

    [HttpPost]
    [Authorize(Policy = "Sales.Opportunity.Create")]
    public async Task<IActionResult> CreateOpportunity([FromBody] CreateOpportunityRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOpportunityCommand(
            request.Title,
            request.Description,
            request.LeadId,
            request.CustomerId,
            request.CompanyId,
            request.ContactId,
            request.Stage,
            request.ValueAmount ?? 0m,
            request.ValueCurrency ?? "USD",
            request.Probability,
            request.ExpectedCloseDate,
            request.AssignedTo,
            request.Source
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Opportunity validation failed.", errors);
        }

        var result = await _createOpportunityHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetOpportunityById), new { id = result.Id }, ResponseFactory.Success(result, "Opportunity created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "Sales.Opportunity.Read")]
    public async Task<IActionResult> GetOpportunities(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? stage = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? leadId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? assignedTo = null,
        [FromQuery] decimal? minValue = null,
        [FromQuery] decimal? maxValue = null,
        [FromQuery] DateOnly? fromCloseDate = null,
        [FromQuery] DateOnly? toCloseDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOpportunitiesQuery(
            page,
            pageSize,
            search,
            stage,
            status,
            customerId,
            leadId,
            companyId,
            assignedTo,
            minValue,
            maxValue,
            fromCloseDate,
            toCloseDate,
            sortBy,
            sortDescending
        );

        var result = await _getOpportunitiesHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Opportunities retrieved successfully."));
    }

    [HttpGet("search")]
    [Authorize(Policy = "Sales.Opportunity.Read")]
    public async Task<IActionResult> SearchOpportunities(
        [FromQuery] string? search = null,
        [FromQuery] string? stage = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? leadId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchOpportunitiesQuery(search, stage, status, customerId, leadId, companyId, page, pageSize);
        var result = await _searchOpportunitiesHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Search results retrieved successfully."));
    }

    [HttpGet("pipeline")]
    [Authorize(Policy = "Sales.Opportunity.Read")]
    public async Task<IActionResult> GetOpportunityPipeline(
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOpportunityPipelineQuery(companyId, customerId, assignedTo);
        var result = await _getPipelineHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Opportunity pipeline retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Sales.Opportunity.Read")]
    public async Task<IActionResult> GetOpportunityById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOpportunityByIdQuery(id);
        var result = await _getByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Opportunity details retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Sales.Opportunity.Update")]
    public async Task<IActionResult> UpdateOpportunity(Guid id, [FromBody] UpdateOpportunityRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateOpportunityCommand(
            id,
            request.Title,
            request.Description,
            request.ValueAmount,
            request.ValueCurrency,
            request.Probability,
            request.ExpectedCloseDate,
            request.Source,
            request.ContactId
        );

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Opportunity update validation failed.", errors);
        }

        var result = await _updateOpportunityHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Opportunity updated successfully."));
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "Sales.Opportunity.Assign")]
    public async Task<IActionResult> AssignOpportunity(Guid id, [FromBody] AssignOpportunityRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignOpportunityCommand(id, request.AssignedTo);

        var validationResult = await _assignValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Opportunity assignment validation failed.", errors);
        }

        var result = await _assignOpportunityHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Opportunity assigned successfully."));
    }

    [HttpPost("{id:guid}/stage")]
    [Authorize(Policy = "Sales.Opportunity.StageChange")]
    public async Task<IActionResult> ChangeOpportunityStage(Guid id, [FromBody] ChangeOpportunityStageRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeOpportunityStageCommand(id, request.Stage, request.Probability);

        var validationResult = await _changeStageValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Opportunity stage change validation failed.", errors);
        }

        var result = await _changeStageHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Opportunity stage updated successfully."));
    }

    [HttpPost("{id:guid}/won")]
    [Authorize(Policy = "Sales.Opportunity.Close")]
    public async Task<IActionResult> MarkOpportunityWon(Guid id, [FromBody] MarkOpportunityWonRequest? request, CancellationToken cancellationToken)
    {
        var command = new MarkOpportunityWonCommand(id, request?.WonAt);

        var validationResult = await _markWonValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Mark won validation failed.", errors);
        }

        var result = await _markWonHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Opportunity marked as won successfully."));
    }

    [HttpPost("{id:guid}/lost")]
    [Authorize(Policy = "Sales.Opportunity.Close")]
    public async Task<IActionResult> MarkOpportunityLost(Guid id, [FromBody] MarkOpportunityLostRequest request, CancellationToken cancellationToken)
    {
        var command = new MarkOpportunityLostCommand(id, request.LossReason, request.LostAt);

        var validationResult = await _markLostValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Mark lost validation failed.", errors);
        }

        var result = await _markLostHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Opportunity marked as lost successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Sales.Opportunity.Delete")]
    public async Task<IActionResult> ArchiveOpportunity(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? archivedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new ArchiveOpportunityCommand(id, archivedBy);
        await _archiveOpportunityHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Opportunity archived successfully.", string.Empty));
    }
}
