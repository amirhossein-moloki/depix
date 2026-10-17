using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Domain.Repositories;

namespace Modules.Identity.Application.Services;

public interface IAuthUseCaseService
{
    Task<Guid> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<List<PendingUserDto>> GetPendingRegistrationsAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetAssignableRolesAsync(CancellationToken cancellationToken = default);
    Task ApproveRegistrationAsync(Guid userId, Guid reviewerId, string roleName, CancellationToken cancellationToken = default);
    Task RejectRegistrationAsync(Guid userId, Guid reviewerId, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginAsync(LoginRequest request, string? clientIp = null, CancellationToken cancellationToken = default);
    Task<LoginResult> RefreshTokenAsync(RefreshTokenRequest request, string? clientIp = null, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
    Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class AuthUseCaseService : IAuthUseCaseService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserApprovalRepository _approvalRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IAuditEventLogger _auditEventLogger;

    public AuthUseCaseService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserApprovalRepository approvalRepository,
        IPermissionRepository permissionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasherService passwordHasherService,
        IAuditEventLogger auditEventLogger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _approvalRepository = approvalRepository;
        _permissionRepository = permissionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasherService = passwordHasherService;
        _auditEventLogger = auditEventLogger;
    }

    public async Task<Guid> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            throw new ValidationException("First and last name are required.");
        if (string.IsNullOrWhiteSpace(request.Email) || !System.Net.Mail.MailAddress.TryCreate(request.Email.Trim(), out _))
            throw new ValidationException("A valid email address is required.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 12)
            throw new ValidationException("Password must contain at least 12 characters.");
        if (request.FirstName.Trim().Length > 100 || request.LastName.Trim().Length > 100 ||
            request.Email.Trim().Length > 250 || (request.Phone?.Length ?? 0) > 50)
            throw new ValidationException("One or more fields exceed the allowed length.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.GetByEmailAsync(email, cancellationToken) != null)
            throw new ValidationException("An account with this email already exists.");

        var user = User.Create(request.FirstName.Trim(), request.LastName.Trim(), email,
            _passwordHasherService.HashPassword(request.Password), request.Phone?.Trim());
        user.SetStatus(UserStatus.Inactive);
        var approval = UserApproval.CreatePending(user.Id);
        await _userRepository.AddRegistrationAsync(user, approval, cancellationToken);
        await _auditEventLogger.LogAsync("REGISTRATION_PENDING", user.Id, "A new registration is awaiting administrator approval.", cancellationToken);
        return user.Id;
    }

    public async Task<List<string>> GetAssignableRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        return roles.Select(r => r.Name)
            .Where(name => !name.Equals("Super Admin", StringComparison.OrdinalIgnoreCase) &&
                           !name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name)
            .ToList();
    }

    public async Task<List<PendingUserDto>> GetPendingRegistrationsAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _approvalRepository.GetPendingAsync(cancellationToken);
        var result = new List<PendingUserDto>();
        foreach (var approval in pending)
        {
            var user = await _userRepository.GetByIdAsync(approval.UserId, cancellationToken);
            if (user != null)
                result.Add(new PendingUserDto(user.Id, user.FirstName, user.LastName, user.Email, user.Phone, approval.CreatedAt, approval.Status.ToString()));
        }
        return result;
    }

    public async Task ApproveRegistrationAsync(Guid userId, Guid reviewerId, string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ValidationException("A role must be selected when approving a registration.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new EntityNotFoundException("User", userId);
        var approval = await _approvalRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ValidationException("This user does not have a pending registration.");
        if (approval.Status != UserApprovalStatus.Pending)
            throw new ValidationException("This registration has already been reviewed.");

        var role = await _roleRepository.GetByNameAsync(roleName.Trim(), cancellationToken)
            ?? throw new ValidationException("The requested role does not exist.");
        if (role.Name.Equals("Super Admin", StringComparison.OrdinalIgnoreCase) ||
            role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Public registrations cannot be assigned an administrator role.");

        user.AddRole(role.Id);
        user.SetStatus(UserStatus.Active);
        approval.Approve(reviewerId);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _approvalRepository.UpdateAsync(approval, cancellationToken);
        await _auditEventLogger.LogAsync("REGISTRATION_APPROVED", reviewerId, $"Approved user {user.Id} and assigned role {role.Name}.", cancellationToken);
    }

    public async Task RejectRegistrationAsync(Guid userId, Guid reviewerId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new EntityNotFoundException("User", userId);
        var approval = await _approvalRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new ValidationException("This user does not have a registration approval record.");
        if (approval.Status != UserApprovalStatus.Pending)
            throw new ValidationException("This registration has already been reviewed.");

        user.SetStatus(UserStatus.Inactive);
        approval.Reject(reviewerId);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _approvalRepository.UpdateAsync(approval, cancellationToken);
        await _auditEventLogger.LogAsync("REGISTRATION_REJECTED", reviewerId, $"Rejected registration for user {user.Id}.", cancellationToken);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, string? clientIp = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Email and password are required.");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        var approval = user == null ? null : await _approvalRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (user == null || user.Status != UserStatus.Active ||
            (approval != null && approval.Status != UserApprovalStatus.Approved) ||
            !_passwordHasherService.VerifyPassword(request.Password, user.PasswordHash))
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
        var approval = user == null ? null : await _approvalRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (user == null || user.Status != UserStatus.Active ||
            (approval != null && approval.Status != UserApprovalStatus.Approved))
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
