using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.SalesVisits.Commands;
using Modules.CRM.Application.Features.SalesVisits.DTOs;
using Modules.CRM.Application.Features.SalesVisits.Queries;

namespace Modules.CRM.API.Controllers;

[ApiController]
[Route("api/crm/visits")]
[Authorize]
public class SalesVisitsController : ControllerBase
{
    private readonly ICommandHandler<CreateSalesVisitCommand, SalesVisitDto> _createVisitHandler;
    private readonly IQueryHandler<GetSalesVisitsQuery, PagedResult<SalesVisitListDto>> _getVisitsHandler;
    private readonly IQueryHandler<GetCompanyVisitHistoryQuery, List<SalesVisitDto>> _getCompanyVisitHistoryHandler;
    private readonly IValidator<CreateSalesVisitCommand> _createValidator;

    public SalesVisitsController(
        ICommandHandler<CreateSalesVisitCommand, SalesVisitDto> createVisitHandler,
        IQueryHandler<GetSalesVisitsQuery, PagedResult<SalesVisitListDto>> getVisitsHandler,
        IQueryHandler<GetCompanyVisitHistoryQuery, List<SalesVisitDto>> getCompanyVisitHistoryHandler,
        IValidator<CreateSalesVisitCommand> createValidator)
    {
        _createVisitHandler = createVisitHandler;
        _getVisitsHandler = getVisitsHandler;
        _getCompanyVisitHistoryHandler = getCompanyVisitHistoryHandler;
        _createValidator = createValidator;
    }

    [HttpPost]
    [Authorize(Policy = "CRM.Visit.Create")]
    public async Task<IActionResult> CreateVisit([FromBody] CreateSalesVisitRequest request, CancellationToken cancellationToken)
    {
        var salespersonIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(salespersonIdClaim, out var salespersonId) || salespersonId == Guid.Empty)
        {
            return Unauthorized(ApiResponse.FailureResponse("Authenticated user ID could not be identified."));
        }

        var command = new CreateSalesVisitCommand(
            request.CompanyId,
            salespersonId,
            request.VisitedAt == default ? DateTime.UtcNow : request.VisitedAt,
            request.ReportText,
            request.Result,
            request.FollowUpAt,
            request.Latitude,
            request.Longitude,
            request.LocationSource
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Sales visit validation failed.", errors);
        }

        var visit = await _createVisitHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetVisits), new { companyId = visit.CompanyId }, ResponseFactory.Success(visit, "Sales visit report registered successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "CRM.Visit.Read")]
    public async Task<IActionResult> GetVisits(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? salespersonId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? resultFilter = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSalesVisitsQuery(page, pageSize, companyId, salespersonId, fromDate, toDate, resultFilter);
        var result = await _getVisitsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Sales visits retrieved successfully."));
    }

    [HttpGet("/api/crm/companies/{companyId:guid}/visits")]
    [Authorize(Policy = "CRM.Visit.Read")]
    public async Task<IActionResult> GetCompanyVisitHistory(Guid companyId, CancellationToken cancellationToken)
    {
        var query = new GetCompanyVisitHistoryQuery(companyId);
        var visits = await _getCompanyVisitHistoryHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(visits, "Company visit history retrieved successfully."));
    }
}
