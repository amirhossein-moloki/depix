using System.Security.Claims;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Application.Services;

namespace Modules.Identity.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IAuthUseCaseService _authUseCaseService;

    public UsersController(IAuthUseCaseService authUseCaseService)
    {
        _authUseCaseService = authUseCaseService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserByAdminRequest request, CancellationToken cancellationToken)
    {
        var user = await _authUseCaseService.CreateUserByAdminAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPendingUsers), new { id = user.Id }, ApiResponse<UserDto>.SuccessResponse(user, "User created successfully."));
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingUsers(CancellationToken cancellationToken)
    {
        var pendingUsers = await _authUseCaseService.GetPendingUsersAsync(cancellationToken);
        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(pendingUsers, "Pending users retrieved successfully."));
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveUser(Guid id, CancellationToken cancellationToken)
    {
        var currentAdminId = GetCurrentUserId();
        var approvedUser = await _authUseCaseService.ApproveUserAsync(id, currentAdminId, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(approvedUser, "User approved successfully."));
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RejectUser(Guid id, [FromBody] ApproveUserRequest? request, CancellationToken cancellationToken)
    {
        var currentAdminId = GetCurrentUserId();
        var rejectedUser = await _authUseCaseService.RejectUserAsync(id, currentAdminId, request?.Reason, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(rejectedUser, "User rejected successfully."));
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
