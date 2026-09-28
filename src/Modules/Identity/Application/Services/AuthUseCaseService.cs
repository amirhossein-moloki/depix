using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Domain.Repositories;

namespace Modules.Identity.Application.Services;

public interface IAuthUseCaseService
{
    Task<LoginResult> LoginAsync(LoginRequest request, string? clientIp = null, CancellationToken cancellationToken = default);
    Task<LoginResult> RefreshTokenAsync(RefreshTokenRequest request, string? clientIp = null, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
    Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class AuthUseCaseService : IAuthUseCaseService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IAuditEventLogger _auditEventLogger;

    public AuthUseCaseService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasherService passwordHasherService,
        IAuditEventLogger auditEventLogger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasherService = passwordHasherService;
        _auditEventLogger = auditEventLogger;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, string? clientIp = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Email and password are required.");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || user.Status != UserStatus.Active || !_passwordHasherService.VerifyPassword(request.Password, user.PasswordHash))
        {
            await _auditEventLogger.LogAsync("LOGIN_FAILURE", user?.Id, $"Failed login attempt for email: {request.Email}", cancellationToken);
            throw new UnauthorizedException("Invalid email or password.");
        }

        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        var roleIds = roles.Select(r => r.Id).ToList();
        var permissions = await _permissionRepository.GetPermissionsByRoleIdsAsync(roleIds, cancellationToken);

        var roleNames = roles.Select(r => r.Name).Distinct().ToList();
        var permissionCodes = permissions.Select(p => p.Code).Distinct().ToList();

        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, roleNames, permissionCodes);
        var rawRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var hashedToken = _jwtTokenService.HashRefreshToken(rawRefreshToken);

        var refreshTokenEntity = RefreshToken.Create(user.Id, hashedToken, TimeSpan.FromDays(7), clientIp);
        user.AddRefreshToken(refreshTokenEntity);
        user.RecordLogin();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _auditEventLogger.LogAsync("LOGIN_SUCCESS", user.Id, "User logged in successfully", cancellationToken);

        return new LoginResult(
            accessToken,
            rawRefreshToken,
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            roleNames,
            permissionCodes
        );
    }

    public async Task<LoginResult> RefreshTokenAsync(RefreshTokenRequest request, string? clientIp = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ValidationException("Refresh token is required.");
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);
        var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user == null || user.Status != UserStatus.Active)
        {
            throw new UnauthorizedException("User account is inactive or not found.");
        }

        // Rotate Refresh Token
        refreshToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);

        var newRawRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newHashedToken = _jwtTokenService.HashRefreshToken(newRawRefreshToken);
        var newRefreshTokenEntity = RefreshToken.Create(user.Id, newHashedToken, TimeSpan.FromDays(7), clientIp);

        user.AddRefreshToken(newRefreshTokenEntity);
        await _userRepository.UpdateAsync(user, cancellationToken);

        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        var roleIds = roles.Select(r => r.Id).ToList();
        var permissions = await _permissionRepository.GetPermissionsByRoleIdsAsync(roleIds, cancellationToken);

        var roleNames = roles.Select(r => r.Name).Distinct().ToList();
        var permissionCodes = permissions.Select(p => p.Code).Distinct().ToList();

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, roleNames, permissionCodes);

        await _auditEventLogger.LogAsync("REFRESH_TOKEN", user.Id, "Refresh token rotated successfully", cancellationToken);

        return new LoginResult(
            newAccessToken,
            newRawRefreshToken,
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            roleNames,
            permissionCodes
        );
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return;
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);
        var refreshToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
            await _auditEventLogger.LogAsync("LOGOUT", refreshToken.UserId, "User logged out", cancellationToken);
        }
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", userId);
        }

        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        var roleIds = roles.Select(r => r.Id).ToList();
        var permissions = await _permissionRepository.GetPermissionsByRoleIdsAsync(roleIds, cancellationToken);

        var roleNames = roles.Select(r => r.Name).Distinct().ToList();
        var permissionCodes = permissions.Select(p => p.Code).Distinct().ToList();

        return new CurrentUserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.Status.ToString(),
            user.LastLoginAt,
            roleNames,
            permissionCodes
        );
    }
}
