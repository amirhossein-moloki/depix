using System.Security.Claims;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Application.Services;

namespace Modules.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly IAuthUseCaseService _authUseCaseService;

    public RolesController(IAuthUseCaseService authUseCaseService)
    {
        _authUseCaseService = authUseCaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var roles = await _authUseCaseService.GetAllRolesAsync(cancellationToken);
        return Ok(ApiResponse<List<RoleDto>>.SuccessResponse(roles, "Roles retrieved successfully."));
    }

    [HttpGet("/api/permissions")]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken)
    {
        var permissions = await _authUseCaseService.GetAllPermissionsAsync(cancellationToken);
        return Ok(ApiResponse<List<PermissionDto>>.SuccessResponse(permissions, "Permissions retrieved successfully."));
    }

    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> UpdateRolePermissions(Guid id, [FromBody] UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var currentAdminId = GetCurrentUserId();
        var updatedRole = await _authUseCaseService.UpdateRolePermissionsAsync(id, currentAdminId, request, cancellationToken);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(updatedRole, "Role permissions updated successfully."));
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
