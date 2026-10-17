using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Application.Services;

namespace Modules.Identity.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Super Admin")]
public class UserApprovalsController : ControllerBase
{
    private readonly IAuthUseCaseService _authService;

    public UserApprovalsController(IAuthUseCaseService authService) => _authService = authService;

    [HttpGet("roles")]
    public async Task<IActionResult> GetAssignableRoles(CancellationToken cancellationToken)
        => Ok(await _authService.GetAssignableRolesAsync(cancellationToken));

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var users = await _authService.GetPendingRegistrationsAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost("{userId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid userId, [FromBody] ReviewRegistrationRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetReviewerId(out var reviewerId)) return Unauthorized();
        await _authService.ApproveRegistrationAsync(userId, reviewerId, request.RoleName, cancellationToken);
        return Ok(new { userId, status = "Approved" });
    }

    [HttpPost("{userId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid userId, CancellationToken cancellationToken)
    {
        if (!TryGetReviewerId(out var reviewerId)) return Unauthorized();
        await _authService.RejectRegistrationAsync(userId, reviewerId, cancellationToken);
        return Ok(new { userId, status = "Rejected" });
    }

    private bool TryGetReviewerId(out Guid reviewerId)
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out reviewerId);
    }
}
