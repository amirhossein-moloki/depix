using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.CRM.Application.Features.Companies;

namespace Modules.CRM.API.Controllers;

[ApiController]
[Route("api/crm/company-visits")]
[Authorize]
public class CompanyVisitsController : ControllerBase
{
    private readonly ICompanyVisitService _service;
    public CompanyVisitsController(ICompanyVisitService service) => _service = service;

    [HttpGet("map-points")]
    [Authorize(Policy = "CRM.Company.Read")]
    public async Task<IActionResult> GetMapPoints(CancellationToken cancellationToken)
        => Ok(await _service.GetMapPointsAsync(cancellationToken));

    [HttpGet("company/{companyId:guid}")]
    [Authorize(Policy = "CRM.Company.Read")]
    public async Task<IActionResult> GetByCompany(Guid companyId, CancellationToken cancellationToken)
        => Ok(await _service.GetByCompanyAsync(companyId, cancellationToken));

    [HttpPost]
    [Authorize(Policy = "CRM.Activity.Create")]
    public async Task<IActionResult> Create([FromBody] CreateCompanyVisitRequest request, CancellationToken cancellationToken)
    {
        var userIdText = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdText, out var sellerId)) return Unauthorized();
        var result = await _service.CreateAsync(sellerId, request, cancellationToken);
        return CreatedAtAction(nameof(GetByCompany), new { companyId = result.CompanyId }, result);
    }
}
