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

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _authUseCaseService.GetAllUsersAsync(cancellationToken);
        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users, "Users retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _authUseCaseService.GetUserByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User details retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserByAdminRequest request, CancellationToken cancellationToken)
    {
        var user = await _authUseCaseService.CreateUserByAdminAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, ApiResponse<UserDto>.SuccessResponse(user, "User created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserByAdminRequest request, CancellationToken cancellationToken)
    {
        var currentAdminId = GetCurrentUserId();
        var updatedUser = await _authUseCaseService.UpdateUserByAdminAsync(id, currentAdminId, request, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(updatedUser, "User updated successfully."));
    }

    [HttpPut("{id:guid}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUserRoles(Guid id, [FromBody] UpdateUserRolesRequest request, CancellationToken cancellationToken)
    {
        var currentAdminId = GetCurrentUserId();
        var updatedUser = await _authUseCaseService.UpdateUserRolesAsync(id, currentAdminId, request, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(updatedUser, "User roles updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var currentAdminId = GetCurrentUserId();
        await _authUseCaseService.DeleteUserByAdminAsync(id, currentAdminId, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("User deleted successfully."));
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

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResetUserPassword(Guid id, [FromBody] AdminResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var currentAdminId = GetCurrentUserId();
        var user = await _authUseCaseService.ResetUserPasswordByAdminAsync(id, currentAdminId, request, cancellationToken);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User password reset successfully."));
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
