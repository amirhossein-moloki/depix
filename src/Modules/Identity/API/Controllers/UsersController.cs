using System.Security.Claims;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Application.Services;

namespace Modules.Identity.API.Controllers;

[ApiController]
[Route("api/identity/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserManagementUseCaseService _userManagementService;

    public UsersController(IUserManagementUseCaseService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    [Authorize(Policy = "Identity.User.Read")]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _userManagementService.GetAllUsersAsync(cancellationToken);
        return Ok(ApiResponse<List<UserSummaryDto>>.SuccessResponse(users, "Users retrieved successfully."));
    }

    [HttpGet("pending")]
    [Authorize(Policy = "Identity.User.Approve")]
    public async Task<IActionResult> GetPendingUsers(CancellationToken cancellationToken)
    {
        var users = await _userManagementService.GetPendingUsersAsync(cancellationToken);
        return Ok(ApiResponse<List<UserSummaryDto>>.SuccessResponse(users, "Pending users retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Policy = "Identity.User.Create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserByAdminRequest request, CancellationToken cancellationToken)
    {
        var adminId = GetCurrentUserId();
        var user = await _userManagementService.CreateUserByAdminAsync(request, adminId, cancellationToken);
        return CreatedAtAction(nameof(GetAllUsers), new { id = user.Id }, ApiResponse<UserSummaryDto>.SuccessResponse(user, "User created successfully by admin."));
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Identity.User.Approve")]
    public async Task<IActionResult> ApproveUser(Guid id, CancellationToken cancellationToken)
    {
        var adminId = GetCurrentUserId();
        var user = await _userManagementService.ApproveUserAsync(id, adminId, cancellationToken);
        return Ok(ApiResponse<UserSummaryDto>.SuccessResponse(user, "User approved successfully."));
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "Identity.User.Approve")]
    public async Task<IActionResult> RejectUser(Guid id, [FromBody] RejectUserRequest? request, CancellationToken cancellationToken)
    {
        var adminId = GetCurrentUserId();
        var user = await _userManagementService.RejectUserAsync(id, adminId, request?.Reason, cancellationToken);
        return Ok(ApiResponse<UserSummaryDto>.SuccessResponse(user, "User rejected successfully."));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return Guid.Empty;
    }
}
