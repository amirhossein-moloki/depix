using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Proposals.Commands;
using Modules.Sales.Application.Features.Proposals.DTOs;
using Modules.Sales.Application.Features.Proposals.Queries;
using ValidationException = BuildingBlocks.Common.Exceptions.ValidationException;

namespace Modules.Sales.API.Controllers;

[ApiController]
[Route("api/sales/proposals")]
[Authorize]
public class ProposalsController : ControllerBase
{
    private readonly ICommandHandler<CreateProposalCommand, ProposalDto> _createProposalHandler;
    private readonly ICommandHandler<UpdateProposalCommand, ProposalDto> _updateProposalHandler;
    private readonly ICommandHandler<AddProposalItemCommand, ProposalDto> _addProposalItemHandler;
    private readonly ICommandHandler<UpdateProposalItemCommand, ProposalDto> _updateProposalItemHandler;
    private readonly ICommandHandler<RemoveProposalItemCommand, ProposalDto> _removeProposalItemHandler;
    private readonly ICommandHandler<ChangeProposalStatusCommand, ProposalDto> _changeProposalStatusHandler;
    private readonly ICommandHandler<ArchiveProposalCommand> _archiveProposalHandler;

    private readonly IQueryHandler<GetProposalByIdQuery, ProposalDto> _getByIdHandler;
    private readonly IQueryHandler<GetProposalsQuery, PagedResult<ProposalListItemDto>> _getProposalsHandler;
    private readonly IQueryHandler<GetOpportunityProposalsQuery, List<ProposalListItemDto>> _getOpportunityProposalsHandler;

    private readonly IValidator<CreateProposalCommand> _createValidator;
    private readonly IValidator<UpdateProposalCommand> _updateValidator;
    private readonly IValidator<AddProposalItemCommand> _addItemValidator;
    private readonly IValidator<UpdateProposalItemCommand> _updateItemValidator;
    private readonly IValidator<ChangeProposalStatusCommand> _changeStatusValidator;

    public ProposalsController(
        ICommandHandler<CreateProposalCommand, ProposalDto> createProposalHandler,
        ICommandHandler<UpdateProposalCommand, ProposalDto> updateProposalHandler,
        ICommandHandler<AddProposalItemCommand, ProposalDto> addProposalItemHandler,
        ICommandHandler<UpdateProposalItemCommand, ProposalDto> updateProposalItemHandler,
        ICommandHandler<RemoveProposalItemCommand, ProposalDto> removeProposalItemHandler,
        ICommandHandler<ChangeProposalStatusCommand, ProposalDto> changeProposalStatusHandler,
        ICommandHandler<ArchiveProposalCommand> archiveProposalHandler,
        IQueryHandler<GetProposalByIdQuery, ProposalDto> getByIdHandler,
        IQueryHandler<GetProposalsQuery, PagedResult<ProposalListItemDto>> getProposalsHandler,
        IQueryHandler<GetOpportunityProposalsQuery, List<ProposalListItemDto>> getOpportunityProposalsHandler,
        IValidator<CreateProposalCommand> createValidator,
        IValidator<UpdateProposalCommand> updateValidator,
        IValidator<AddProposalItemCommand> addItemValidator,
        IValidator<UpdateProposalItemCommand> updateItemValidator,
        IValidator<ChangeProposalStatusCommand> changeStatusValidator)
    {
        _createProposalHandler = createProposalHandler;
        _updateProposalHandler = updateProposalHandler;
        _addProposalItemHandler = addProposalItemHandler;
        _updateProposalItemHandler = updateProposalItemHandler;
        _removeProposalItemHandler = removeProposalItemHandler;
        _changeProposalStatusHandler = changeProposalStatusHandler;
        _archiveProposalHandler = archiveProposalHandler;
        _getByIdHandler = getByIdHandler;
        _getProposalsHandler = getProposalsHandler;
        _getOpportunityProposalsHandler = getOpportunityProposalsHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _addItemValidator = addItemValidator;
        _updateItemValidator = updateItemValidator;
        _changeStatusValidator = changeStatusValidator;
    }

    [HttpPost]
    [Authorize(Policy = "Sales.Proposal.Create")]
    public async Task<IActionResult> CreateProposal([FromBody] CreateProposalRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProposalCommand(
            request.OpportunityId,
            request.Title,
            request.ValidUntil,
            request.Version,
            request.Description,
            request.CustomerId,
            request.CompanyId,
            request.Notes,
            request.Currency
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Proposal creation validation failed.", errors);
        }

        var result = await _createProposalHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetProposalById), new { id = result.Id }, ResponseFactory.Success(result, "Proposal created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "Sales.Proposal.Read")]
    public async Task<IActionResult> GetProposals(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? opportunityId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProposalsQuery(page, pageSize, opportunityId, customerId, companyId, status);
        var result = await _getProposalsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Proposals retrieved successfully."));
    }

    [HttpGet("opportunity/{opportunityId:guid}")]
    [Authorize(Policy = "Sales.Proposal.Read")]
    public async Task<IActionResult> GetOpportunityProposals(Guid opportunityId, CancellationToken cancellationToken)
    {
        var query = new GetOpportunityProposalsQuery(opportunityId);
        var result = await _getOpportunityProposalsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Opportunity proposals retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Sales.Proposal.Read")]
    public async Task<IActionResult> GetProposalById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProposalByIdQuery(id);
        var result = await _getByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Proposal details retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Sales.Proposal.Update")]
    public async Task<IActionResult> UpdateProposal(Guid id, [FromBody] UpdateProposalRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProposalCommand(
            id,
            request.Title,
            request.Description,
            request.ValidUntil,
            request.Version,
            request.Notes
        );

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Proposal update validation failed.", errors);
        }

        var result = await _updateProposalHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Proposal updated successfully."));
    }

    [HttpPost("{id:guid}/items")]
    [Authorize(Policy = "Sales.Proposal.Update")]
    public async Task<IActionResult> AddProposalItem(Guid id, [FromBody] AddProposalItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddProposalItemCommand(
            id,
            request.Name,
            request.Description,
            request.Quantity,
            request.UnitPrice,
            request.Discount
        );

        var validationResult = await _addItemValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Add proposal item validation failed.", errors);
        }

        var result = await _addProposalItemHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Proposal item added successfully."));
    }

    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [Authorize(Policy = "Sales.Proposal.Update")]
    public async Task<IActionResult> UpdateProposalItem(Guid id, Guid itemId, [FromBody] UpdateProposalItemRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProposalItemCommand(
            id,
            itemId,
            request.Name,
            request.Description,
            request.Quantity,
            request.UnitPrice,
            request.Discount
        );

        var validationResult = await _updateItemValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Update proposal item validation failed.", errors);
        }

        var result = await _updateProposalItemHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Proposal item updated successfully."));
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [Authorize(Policy = "Sales.Proposal.Update")]
    public async Task<IActionResult> RemoveProposalItem(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        var command = new RemoveProposalItemCommand(id, itemId);
        var result = await _removeProposalItemHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Proposal item removed successfully."));
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = "Sales.Proposal.StatusChange")]
    public async Task<IActionResult> ChangeProposalStatus(Guid id, [FromBody] ChangeProposalStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeProposalStatusCommand(id, request.Status, request.Reason);

        var validationResult = await _changeStatusValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Proposal status change validation failed.", errors);
        }

        var result = await _changeProposalStatusHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Proposal status updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Sales.Proposal.Delete")]
    public async Task<IActionResult> ArchiveProposal(Guid id, CancellationToken cancellationToken)
    {
        var command = new ArchiveProposalCommand(id);
        await _archiveProposalHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Proposal archived successfully.", string.Empty));
    }
}
