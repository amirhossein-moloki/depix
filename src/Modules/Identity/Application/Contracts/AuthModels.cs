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

public record RefreshTokenRequest(string RefreshToken);

public record LogoutRequest(string RefreshToken);

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

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone
);

public record RegisterResult(
    Guid UserId,
    string Email,
    string Status,
    string Message
);

public record CreateUserByAdminRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone,
    string? RoleName
);

public record RejectUserRequest(
    string? Reason
);

public record UserSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string Status,
    DateTime CreatedAt,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    string? RejectionReason,
    List<string> Roles
);
