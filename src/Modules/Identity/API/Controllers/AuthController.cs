using System.Security.Claims;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Application.Services;

namespace Modules.Identity.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthUseCaseService _authUseCaseService;

    public AuthController(IAuthUseCaseService authUseCaseService)
    {
        _authUseCaseService = authUseCaseService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authUseCaseService.LoginAsync(request, clientIp, cancellationToken);
        return Ok(ApiResponse<LoginResult>.SuccessResponse(result, "Logged in successfully."));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authUseCaseService.RefreshTokenAsync(request, clientIp, cancellationToken);
        return Ok(ApiResponse<LoginResult>.SuccessResponse(result, "Token refreshed successfully."));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authUseCaseService.LogoutAsync(request, cancellationToken);
        return Ok(ApiResponse.SuccessResponse("Logged out successfully."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.FailureResponse("Unauthorized access."));
        }

        var user = await _authUseCaseService.GetCurrentUserAsync(userId, cancellationToken);
        return Ok(ApiResponse<CurrentUserDto>.SuccessResponse(user, "User details retrieved successfully."));
    }
}
