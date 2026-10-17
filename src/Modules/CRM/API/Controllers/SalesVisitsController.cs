using System.Security.Claims;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.CRM.Application.DTOs;
using Modules.CRM.Application.Services;

namespace Modules.CRM.API.Controllers;

[ApiController]
[Route("api/crm/sales-visits")]
[Authorize]
public class SalesVisitsController : ControllerBase
{
    private readonly ISalesVisitService _salesVisitService;

    public SalesVisitsController(ISalesVisitService salesVisitService)
    {
        _salesVisitService = salesVisitService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateVisit([FromBody] CreateSalesVisitRequest request, CancellationToken cancellationToken)
    {
        var salespersonId = GetCurrentUserId();
        var visit = await _salesVisitService.CreateVisitAsync(salespersonId, request, cancellationToken);
        return CreatedAtAction(nameof(GetVisitById), new { id = visit.Id }, ApiResponse<SalesVisitDto>.SuccessResponse(visit, "Sales visit recorded successfully."));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVisitById(Guid id, CancellationToken cancellationToken)
    {
        var visit = await _salesVisitService.GetVisitByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SalesVisitDto>.SuccessResponse(visit, "Sales visit retrieved successfully."));
    }

    [HttpGet]
    public async Task<IActionResult> GetVisits(
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? salespersonId,
        [FromQuery] string? result,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var visits = await _salesVisitService.GetVisitsAsync(companyId, salespersonId, result, fromDate, toDate, page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<SalesVisitDto>>.SuccessResponse(visits, "Sales visits retrieved successfully."));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Current user ID not found in token claims.");
        }
        return userId;
    }
}
