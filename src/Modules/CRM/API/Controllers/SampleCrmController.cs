using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Modules.CRM.API.Controllers;

[ApiController]
[Route("api/crm/sample")]
public class SampleCrmController : ControllerBase
{
    public record SampleLeadDto(Guid Id, string CompanyName, string ContactEmail);

    [HttpGet("success")]
    public ActionResult<ApiResponse<SampleLeadDto>> GetSuccess()
    {
        var lead = new SampleLeadDto(Guid.NewGuid(), "Acme Corp", "contact@acme.com");
        return Ok(ResponseFactory.Success(lead, "Lead retrieved successfully", HttpContext.TraceIdentifier));
    }

    [HttpGet("validation-error")]
    public IActionResult TriggerValidationError()
    {
        throw new ValidationException("Validation failed for lead request.", new[]
        {
            new ValidationErrorDetail("email", "Email is invalid"),
            new ValidationErrorDetail("companyName", "Company name is required")
        }, ErrorCode.ValidationError);
    }

    [HttpGet("business-error")]
    public IActionResult TriggerBusinessError()
    {
        throw new BusinessRuleException(
            "Lead cannot be converted without an active contact.",
            ErrorCode.CrmError);
    }

    [HttpGet("not-found")]
    public IActionResult TriggerNotFound()
    {
        throw new EntityNotFoundException("Lead", "12345");
    }

    [HttpGet("unauthorized")]
    public IActionResult TriggerUnauthorized()
    {
        throw new UnauthorizedException("User is not authenticated.", ErrorCode.AuthError);
    }

    [HttpGet("forbidden")]
    public IActionResult TriggerForbidden()
    {
        throw new ForbiddenException("User does not have permission to delete lead.");
    }

    [HttpGet("unexpected-error")]
    public IActionResult TriggerUnexpectedError()
    {
        throw new InvalidOperationException("Simulated unexpected database failure.");
    }
}
