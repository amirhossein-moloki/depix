namespace Modules.Identity.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    string HashRefreshToken(string rawToken);
}

public interface IPasswordHasherService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public interface IAuditEventLogger
{
    Task LogAsync(string eventType, Guid? userId, string details, CancellationToken cancellationToken = default);
}
