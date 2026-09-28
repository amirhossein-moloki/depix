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
