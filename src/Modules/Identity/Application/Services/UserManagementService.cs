using BuildingBlocks.Common.Exceptions;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Domain.Repositories;

namespace Modules.Identity.Application.Services;

public interface IUserManagementUseCaseService
{
    Task<RegisterResult> RegisterPublicAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<UserSummaryDto> CreateUserByAdminAsync(CreateUserByAdminRequest request, Guid adminId, CancellationToken cancellationToken = default);
    Task<List<UserSummaryDto>> GetPendingUsersAsync(CancellationToken cancellationToken = default);
    Task<List<UserSummaryDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserSummaryDto> ApproveUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default);
    Task<UserSummaryDto> RejectUserAsync(Guid userId, Guid adminId, string? reason, CancellationToken cancellationToken = default);
}

public class UserManagementService : IUserManagementUseCaseService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IAuditEventLogger _auditEventLogger;

    public UserManagementService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasherService passwordHasherService,
        IAuditEventLogger auditEventLogger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasherService = passwordHasherService;
        _auditEventLogger = auditEventLogger;
    }

    public async Task<RegisterResult> RegisterPublicAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Email and password are required.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ValidationException("First name and last name are required.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new ValidationException("A user with this email address already exists.");
        }

        var passwordHash = _passwordHasherService.HashPassword(request.Password);
        var user = User.CreatePending(request.FirstName, request.LastName, request.Email, passwordHash, request.Phone);

        // Assign default User / SalesPerson role if present
        var roles = await _roleRepository.GetAllAsync(cancellationToken) ?? new List<Role>();
        var defaultRole = roles.FirstOrDefault(r => r.Name.Equals("SalesPerson", StringComparison.OrdinalIgnoreCase))
                       ?? roles.FirstOrDefault(r => r.Name.Equals("User", StringComparison.OrdinalIgnoreCase));

        if (defaultRole != null)
        {
            user.AddRole(defaultRole.Id);
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _auditEventLogger.LogAsync("USER_REGISTERED_PUBLIC", user.Id, $"Public user registered with status PendingApproval", cancellationToken);

        return new RegisterResult(
            user.Id,
            user.Email,
            user.Status.ToString(),
            "Registration successful. Your account is pending administrator approval before you can log in."
        );
    }

    public async Task<UserSummaryDto> CreateUserByAdminAsync(CreateUserByAdminRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Email and password are required.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ValidationException("First name and last name are required.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new ValidationException("A user with this email address already exists.");
        }

        var passwordHash = _passwordHasherService.HashPassword(request.Password);
        var user = User.Create(request.FirstName, request.LastName, request.Email, passwordHash, request.Phone);
        user.Approve(adminId); // Admin-created user is automatically approved/active

        var targetRoleName = string.IsNullOrWhiteSpace(request.RoleName) ? "SalesPerson" : request.RoleName.Trim();
        var roles = await _roleRepository.GetAllAsync(cancellationToken) ?? new List<Role>();
        var role = roles.FirstOrDefault(r => r.Name.Equals(targetRoleName, StringComparison.OrdinalIgnoreCase));

        if (role == null)
        {
            role = roles.FirstOrDefault(r => r.Name.Equals("User", StringComparison.OrdinalIgnoreCase));
        }

        if (role != null)
        {
            user.AddRole(role.Id);
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _auditEventLogger.LogAsync("USER_CREATED_BY_ADMIN", user.Id, $"User created by admin {adminId}", cancellationToken);

        return await MapToUserSummaryDto(user, cancellationToken);
    }

    public async Task<List<UserSummaryDto>> GetPendingUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetUsersByStatusAsync(UserStatus.PendingApproval, cancellationToken) ?? new List<User>();
        var result = new List<UserSummaryDto>();
        foreach (var user in users)
        {
            result.Add(await MapToUserSummaryDto(user, cancellationToken));
        }
        return result;
    }

    public async Task<List<UserSummaryDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllUsersAsync(cancellationToken) ?? new List<User>();
        var result = new List<UserSummaryDto>();
        foreach (var user in users)
        {
            result.Add(await MapToUserSummaryDto(user, cancellationToken));
        }
        return result;
    }

    public async Task<UserSummaryDto> ApproveUserAsync(Guid userId, Guid adminId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", userId);
        }

        user.Approve(adminId);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _auditEventLogger.LogAsync("USER_APPROVED", user.Id, $"User approved by admin {adminId}", cancellationToken);

        return await MapToUserSummaryDto(user, cancellationToken);
    }

    public async Task<UserSummaryDto> RejectUserAsync(Guid userId, Guid adminId, string? reason, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException("User", userId);
        }

        user.Reject(adminId, reason);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _auditEventLogger.LogAsync("USER_REJECTED", user.Id, $"User rejected by admin {adminId}. Reason: {reason}", cancellationToken);

        return await MapToUserSummaryDto(user, cancellationToken);
    }

    private async Task<UserSummaryDto> MapToUserSummaryDto(User user, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetRolesByUserIdAsync(user.Id, cancellationToken) ?? new List<Role>();
        var roleNames = roles.Select(r => r.Name).Distinct().ToList();

        return new UserSummaryDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Phone,
            user.Status.ToString(),
            user.CreatedAt,
            user.ApprovedBy,
            user.ApprovedAt,
            user.RejectionReason,
            roleNames
        );
    }
}
