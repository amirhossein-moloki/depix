namespace Modules.Identity.Application.Contracts;

public record LoginRequest(string Email, string Password);

public record LoginResult(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    List<string> Roles,
    List<string> Permissions
);

public record ResetPasswordRequest(string CurrentPassword, string NewPassword);

public record AdminResetPasswordRequest(string NewPassword);

public record RefreshTokenRequest(string RefreshToken);

public record LogoutRequest(string RefreshToken);

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone
);

public record CreateUserByAdminRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone,
    List<string>? RoleNames
);

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Status,
    DateTime CreatedAt,
    List<string> Roles,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    string? RejectionReason
);

public record ApproveUserRequest(string? Reason);

public record CurrentUserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Status,
    DateTime? LastLoginAt,
    List<string> Roles,
    List<string> Permissions
);
