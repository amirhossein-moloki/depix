using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using Microsoft.Extensions.Logging;
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
    Task<UserDto> RegisterPublicUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserByAdminAsync(CreateUserByAdminRequest request, CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserByAdminAsync(Guid targetUserId, Guid adminUserId, UpdateUserByAdminRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserRolesAsync(Guid targetUserId, Guid adminUserId, UpdateUserRolesRequest request, CancellationToken cancellationToken = default);
    Task DeleteUserByAdminAsync(Guid targetUserId, Guid adminUserId, CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetPendingUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> ApproveUserAsync(Guid targetUserId, Guid adminUserId, CancellationToken cancellationToken = default);
    Task<UserDto> RejectUserAsync(Guid targetUserId, Guid adminUserId, string? reason = null, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> ResetUserPasswordByAdminAsync(Guid targetUserId, Guid adminUserId, AdminResetPasswordRequest request, CancellationToken cancellationToken = default);

    // Role & Permission Management
    Task<List<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<List<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateRolePermissionsAsync(Guid roleId, Guid adminUserId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default);
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
    private readonly ILogger<AuthUseCaseService> _logger;

    public AuthUseCaseService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasherService passwordHasherService,
        IAuditEventLogger auditEventLogger,
        ILogger<AuthUseCaseService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasherService = passwordHasherService;
        _auditEventLogger = auditEventLogger;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, string? clientIp = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Email and password are required.");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user == null || !_passwordHasherService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user email {Email}", request.Email);
            await _auditEventLogger.LogAsync("LOGIN_FAILURE", user?.Id, $"Failed login attempt for email: {request.Email}", cancellationToken);
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (user.Status == UserStatus.PendingApproval)
        {
            _logger.LogWarning("User {UserId} attempted login while status is PendingApproval", user.Id);
            await _auditEventLogger.LogAsync("LOGIN_FAILURE", user.Id, "Attempted login while pending admin approval.", cancellationToken);
            throw new ForbiddenException("Account is pending administrator approval.");
        }

        if (user.Status == UserStatus.Rejected)
        {
            _logger.LogWarning("User {UserId} attempted login on rejected account", user.Id);
            await _auditEventLogger.LogAsync("LOGIN_FAILURE", user.Id, "Attempted login on rejected account.", cancellationToken);
            throw new ForbiddenException("Account registration request was rejected by an administrator.");
        }

        if (user.Status != UserStatus.Active)
        {
            _logger.LogWarning("User {UserId} attempted login on inactive/locked account ({Status})", user.Id, user.Status);
            await _auditEventLogger.LogAsync("LOGIN_FAILURE", user.Id, "Attempted login on inactive or locked account.", cancellationToken);
            throw new ForbiddenException("Account is inactive or locked out.");
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
        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
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
            refreshToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
            throw new UnauthorizedException("User account is inactive, pending approval, rejected, or not found.");
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

        _logger.LogInformation("Refresh token rotated successfully for user {UserId}", user.Id);
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
            _logger.LogInformation("User {UserId} logged out successfully", refreshToken.UserId);
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

    public async Task<UserDto> RegisterPublicUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Email and password are required for registration.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new BusinessRuleException("User with this email already exists.");
        }

        var hashedPassword = _passwordHasherService.HashPassword(request.Password);
        var newUser = User.Create(request.FirstName, request.LastName, request.Email, hashedPassword, request.Phone, UserStatus.Active);

        var allRoles = await _roleRepository.GetAllAsync(cancellationToken);
        var defaultRole = allRoles.FirstOrDefault(r => r.Name.Equals("User", StringComparison.OrdinalIgnoreCase))
                          ?? allRoles.FirstOrDefault(r => !r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase));

        if (defaultRole != null)
        {
            newUser.AddRole(defaultRole.Id);
        }

        await _userRepository.AddAsync(newUser, cancellationToken);
        _logger.LogInformation("User {UserId} registered via public portal with Active status", newUser.Id);
        await _auditEventLogger.LogAsync("USER_REGISTERED", newUser.Id, "User registered via public portal.", cancellationToken);

        var userRoles = await _roleRepository.GetRolesByUserIdAsync(newUser.Id, cancellationToken);
        return MapToUserDto(newUser, userRoles.Select(r => r.Name).ToList());
    }

    public async Task<UserDto> CreateUserByAdminAsync(CreateUserByAdminRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Email and password are required.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new BusinessRuleException("User with this email already exists.");
        }

        var hashedPassword = _passwordHasherService.HashPassword(request.Password);
        var newUser = User.Create(request.FirstName, request.LastName, request.Email, hashedPassword, request.Phone, UserStatus.Active);

        var allRoles = await _roleRepository.GetAllAsync(cancellationToken);
        if (request.RoleNames != null && request.RoleNames.Any())
        {
            foreach (var roleName in request.RoleNames.Distinct())
            {
                var role = allRoles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                if (role != null)
                {
                    newUser.AddRole(role.Id);
                }
            }
        }

        if (!newUser.UserRoles.Any())
        {
            var defaultRole = allRoles.FirstOrDefault(r => r.Name.Equals("User", StringComparison.OrdinalIgnoreCase));
            if (defaultRole != null)
            {
                newUser.AddRole(defaultRole.Id);
            }
        }

        await _userRepository.AddAsync(newUser, cancellationToken);
        _logger.LogInformation("User {UserId} created by administrator", newUser.Id);
        await _auditEventLogger.LogAsync("USER_CREATED_BY_ADMIN", newUser.Id, "User created by administrator.", cancellationToken);

        var assignedRoles = await _roleRepository.GetRolesByUserIdAsync(newUser.Id, cancellationToken);
        return MapToUserDto(newUser, assignedRoles.Select(r => r.Name).ToList());
    }

    public async Task<List<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var dtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
            dtos.Add(MapToUserDto(user, roles.Select(r => r.Name).ToList()));
        }

        return dtos;
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", userId);
        }

        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        return MapToUserDto(user, roles.Select(r => r.Name).ToList());
    }

    public async Task<UserDto> UpdateUserByAdminAsync(Guid targetUserId, Guid adminUserId, UpdateUserByAdminRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email is required.");
        }

        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", targetUserId);
        }

        var existingUserWithEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUserWithEmail != null && existingUserWithEmail.Id != user.Id)
        {
            throw new BusinessRuleException("Another user with this email already exists.");
        }

        user.UpdateProfile(request.FirstName, request.LastName, request.Email, request.Phone);

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<UserStatus>(request.Status, true, out var newStatus))
        {
            user.SetStatus(newStatus);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        _logger.LogInformation("User {UserId} updated by Admin {AdminId}", user.Id, adminUserId);
        await _auditEventLogger.LogAsync("USER_UPDATED_BY_ADMIN", user.Id, $"User details updated by Admin ID: {adminUserId}", cancellationToken);

        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        return MapToUserDto(user, roles.Select(r => r.Name).ToList());
    }

    public async Task<UserDto> UpdateUserRolesAsync(Guid targetUserId, Guid adminUserId, UpdateUserRolesRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", targetUserId);
        }

        var allRoles = await _roleRepository.GetAllAsync(cancellationToken);
        var matchingRoleIds = new List<Guid>();

        if (request.RoleNames != null && request.RoleNames.Any())
        {
            foreach (var roleName in request.RoleNames.Distinct())
            {
                var role = allRoles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
                if (role != null)
                {
                    matchingRoleIds.Add(role.Id);
                }
            }
        }

        user.SetRoles(matchingRoleIds);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Roles for user {UserId} updated by Admin {AdminId}", user.Id, adminUserId);
        await _auditEventLogger.LogAsync("USER_ROLES_UPDATED_BY_ADMIN", user.Id, $"User roles updated by Admin ID: {adminUserId}", cancellationToken);

        var assignedRoles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        return MapToUserDto(user, assignedRoles.Select(r => r.Name).ToList());
    }

    public async Task DeleteUserByAdminAsync(Guid targetUserId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        if (targetUserId == adminUserId)
        {
            throw new BusinessRuleException("Administrators cannot delete their own account.");
        }

        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", targetUserId);
        }

        var userRoles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        if (userRoles.Any(r => r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
        {
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var adminCount = 0;
            foreach (var u in allUsers)
            {
                var roles = await _roleRepository.GetRolesByUserIdAsync(u.Id, cancellationToken);
                if (roles.Any(r => r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                {
                    adminCount++;
                }
            }

            if (adminCount <= 1)
            {
                throw new BusinessRuleException("Cannot delete the last administrator in the system.");
            }
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        _logger.LogInformation("User {UserId} deleted by Admin {AdminId}", targetUserId, adminUserId);
        await _auditEventLogger.LogAsync("USER_DELETED_BY_ADMIN", targetUserId, $"User deleted by Admin ID: {adminUserId}", cancellationToken);
    }

    public async Task<List<UserDto>> GetPendingUsersAsync(CancellationToken cancellationToken = default)
    {
        var pendingUsers = await _userRepository.GetPendingUsersAsync(cancellationToken);
        var dtos = new List<UserDto>();

        foreach (var user in pendingUsers)
        {
            var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
            dtos.Add(MapToUserDto(user, roles.Select(r => r.Name).ToList()));
        }

        return dtos;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        var allPermissions = await _permissionRepository.GetAllAsync(cancellationToken);
        var permDict = allPermissions.ToDictionary(p => p.Id);

        var result = new List<RoleDto>();
        foreach (var role in roles)
        {
            var permDtos = role.RolePermissions
                .Where(rp => permDict.ContainsKey(rp.PermissionId))
                .Select(rp => permDict[rp.PermissionId])
                .Select(p => new PermissionDto(p.Id, p.Code, p.Name, p.Module))
                .ToList();

            result.Add(new RoleDto(role.Id, role.Name, role.Description, permDtos));
        }

        return result;
    }

    public async Task<List<PermissionDto>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        return permissions.Select(p => new PermissionDto(p.Id, p.Code, p.Name, p.Module)).ToList();
    }

    public async Task<RoleDto> UpdateRolePermissionsAsync(Guid roleId, Guid adminUserId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role == null)
        {
            throw new EntityNotFoundException("Role", roleId);
        }

        await _roleRepository.UpdateRolePermissionsAsync(roleId, request.PermissionIds, cancellationToken);
        _logger.LogInformation("Permissions for Role {RoleId} updated by Admin {AdminId}", roleId, adminUserId);
        await _auditEventLogger.LogAsync("ROLE_PERMISSIONS_UPDATED", roleId, $"Role permissions updated by Admin ID: {adminUserId}", cancellationToken);

        var updatedRole = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        var allPermissions = await _permissionRepository.GetAllAsync(cancellationToken);
        var permDict = allPermissions.ToDictionary(p => p.Id);

        var permDtos = updatedRole!.RolePermissions
            .Where(rp => permDict.ContainsKey(rp.PermissionId))
            .Select(rp => permDict[rp.PermissionId])
            .Select(p => new PermissionDto(p.Id, p.Code, p.Name, p.Module))
            .ToList();

        return new RoleDto(updatedRole.Id, updatedRole.Name, updatedRole.Description, permDtos);
    }

    public async Task<UserDto> ApproveUserAsync(Guid targetUserId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", targetUserId);
        }

        user.Approve(adminUserId);
        await _userRepository.UpdateAsync(user, cancellationToken);
        _logger.LogInformation("User {UserId} approved by Admin {AdminId}", user.Id, adminUserId);
        await _auditEventLogger.LogAsync("USER_APPROVED", user.Id, $"User approved by Admin ID: {adminUserId}", cancellationToken);

        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        return MapToUserDto(user, roles.Select(r => r.Name).ToList());
    }

    public async Task<UserDto> RejectUserAsync(Guid targetUserId, Guid adminUserId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", targetUserId);
        }

        user.Reject(adminUserId, reason);

        foreach (var token in user.RefreshTokens.Where(rt => rt.IsActive))
        {
            token.Revoke();
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        _logger.LogInformation("User {UserId} rejected by Admin {AdminId}", user.Id, adminUserId);
        await _auditEventLogger.LogAsync("USER_REJECTED", user.Id, $"User rejected by Admin ID: {adminUserId}. Reason: {reason}", cancellationToken);

        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        return MapToUserDto(user, roles.Select(r => r.Name).ToList());
    }

    public async Task ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ValidationException("Current password and new password are required.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", userId);
        }

        if (!_passwordHasherService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            await _auditEventLogger.LogAsync("PASSWORD_RESET_FAILURE", user.Id, "Failed password reset attempt due to incorrect current password", cancellationToken);
            throw new UnauthorizedException("Incorrect current password.");
        }

        var newPasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);

        foreach (var token in user.RefreshTokens.Where(rt => rt.IsActive))
        {
            token.Revoke();
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
        await _auditEventLogger.LogAsync("PASSWORD_RESET_SUCCESS", user.Id, "User reset their password successfully", cancellationToken);
    }

    public async Task<UserDto> ResetUserPasswordByAdminAsync(Guid targetUserId, Guid adminUserId, AdminResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ValidationException("New password is required.");
        }

        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", targetUserId);
        }

        var newPasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);

        foreach (var token in user.RefreshTokens.Where(rt => rt.IsActive))
        {
            token.Revoke();
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        _logger.LogInformation("Password for user {UserId} reset by Admin {AdminId}", user.Id, adminUserId);
        await _auditEventLogger.LogAsync("USER_PASSWORD_RESET_BY_ADMIN", user.Id, $"Password reset by Admin ID: {adminUserId}", cancellationToken);

        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken);
        return MapToUserDto(user, roles.Select(r => r.Name).ToList());
    }

    private static UserDto MapToUserDto(User user, List<string> roles)
    {
        return new UserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.Status.ToString(),
            user.CreatedAt,
            roles,
            user.ApprovedBy,
            user.ApprovedAt,
            user.RejectionReason
        );
    }
}
