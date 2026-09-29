using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.SalesNotes.Commands;
using Modules.CRM.Application.Features.SalesNotes.DTOs;
using Modules.CRM.Application.Features.SalesNotes.Queries;
using ValidationException = BuildingBlocks.Common.Exceptions.ValidationException;

namespace Modules.CRM.API.Controllers;

[ApiController]
[Route("api/crm/sales-notes")]
[Authorize]
public class SalesNotesController : ControllerBase
{
    private readonly ICommandHandler<CreateSalesNoteCommand, SalesNoteDto> _createSalesNoteHandler;
    private readonly ICommandHandler<UpdateSalesNoteCommand, SalesNoteDto> _updateSalesNoteHandler;
    private readonly ICommandHandler<ArchiveSalesNoteCommand> _archiveSalesNoteHandler;
    private readonly IQueryHandler<GetSalesNoteByIdQuery, SalesNoteDto> _getSalesNoteByIdHandler;
    private readonly IQueryHandler<GetSalesNotesQuery, PagedResult<SalesNoteListItemDto>> _getSalesNotesHandler;
    private readonly IQueryHandler<GetLeadSalesNotesQuery, LeadSalesContextDto> _getLeadSalesNotesHandler;
    private readonly IValidator<CreateSalesNoteCommand> _createValidator;
    private readonly IValidator<UpdateSalesNoteCommand> _updateValidator;

    public SalesNotesController(
        ICommandHandler<CreateSalesNoteCommand, SalesNoteDto> createSalesNoteHandler,
        ICommandHandler<UpdateSalesNoteCommand, SalesNoteDto> updateSalesNoteHandler,
        ICommandHandler<ArchiveSalesNoteCommand> archiveSalesNoteHandler,
        IQueryHandler<GetSalesNoteByIdQuery, SalesNoteDto> getSalesNoteByIdHandler,
        IQueryHandler<GetSalesNotesQuery, PagedResult<SalesNoteListItemDto>> getSalesNotesHandler,
        IQueryHandler<GetLeadSalesNotesQuery, LeadSalesContextDto> getLeadSalesNotesHandler,
        IValidator<CreateSalesNoteCommand> createValidator,
        IValidator<UpdateSalesNoteCommand> updateValidator)
    {
        _createSalesNoteHandler = createSalesNoteHandler;
        _updateSalesNoteHandler = updateSalesNoteHandler;
        _archiveSalesNoteHandler = archiveSalesNoteHandler;
        _getSalesNoteByIdHandler = getSalesNoteByIdHandler;
        _getSalesNotesHandler = getSalesNotesHandler;
        _getLeadSalesNotesHandler = getLeadSalesNotesHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    [Authorize(Policy = "CRM.SalesNote.Create")]
    public async Task<IActionResult> CreateSalesNote([FromBody] CreateSalesNoteRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid createdBy = Guid.TryParse(userIdClaim, out var uId) ? uId : Guid.Empty;

        var command = new CreateSalesNoteCommand(
            request.LeadId,
            request.Title,
            request.NeedAnalysis,
            request.Objections,
            request.Strategy,
            request.Probability,
            createdBy,
            request.CompanyId,
            request.ContactId,
            request.CompetitorsMentioned,
            request.BudgetInformation,
            request.DecisionMakerInfo
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("SalesNote validation failed.", errors);
        }

        var salesNote = await _createSalesNoteHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetSalesNoteById), new { id = salesNote.Id }, ResponseFactory.Success(salesNote, "SalesNote created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "CRM.SalesNote.Read")]
    public async Task<IActionResult> GetSalesNotes(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? leadId = null,
        [FromQuery] Guid? contactId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? createdBy = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSalesNotesQuery(page, pageSize, leadId, contactId, companyId, createdBy, search);
        var result = await _getSalesNotesHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "SalesNotes retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CRM.SalesNote.Read")]
    public async Task<IActionResult> GetSalesNoteById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSalesNoteByIdQuery(id);
        var salesNote = await _getSalesNoteByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(salesNote, "SalesNote details retrieved successfully."));
    }

    [HttpGet("/api/crm/leads/{leadId:guid}/sales-notes")]
    [Authorize(Policy = "CRM.SalesNote.Read")]
    public async Task<IActionResult> GetLeadSalesNotes(Guid leadId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = new GetLeadSalesNotesQuery(leadId, page, pageSize);
        var result = await _getLeadSalesNotesHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Lead sales notes retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CRM.SalesNote.Update")]
    public async Task<IActionResult> UpdateSalesNote(Guid id, [FromBody] UpdateSalesNoteRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateSalesNoteCommand(
            id,
            request.Title,
            request.NeedAnalysis,
            request.Objections,
            request.Strategy,
            request.Probability,
            request.CompanyId,
            request.ContactId,
            request.CompetitorsMentioned,
            request.BudgetInformation,
            request.DecisionMakerInfo
        );

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("SalesNote update validation failed.", errors);
        }

        var salesNote = await _updateSalesNoteHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(salesNote, "SalesNote updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CRM.SalesNote.Delete")]
    public async Task<IActionResult> ArchiveSalesNote(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? archivedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new ArchiveSalesNoteCommand(id, archivedBy);
        await _archiveSalesNoteHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("SalesNote archived successfully.", string.Empty));
    }
}
