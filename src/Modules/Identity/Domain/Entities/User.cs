using BuildingBlocks.Domain.Models;

namespace Modules.Identity.Domain.Entities;

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    LockedOut = 3
}

public class User : AuditableAggregateRoot
{
    private readonly List<UserRole> _userRoles = new();
    private readonly List<RefreshToken> _refreshTokens = new();

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public DateTime? LastLoginAt { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public User(Guid id, string firstName, string lastName, string email, string passwordHash, string? phone = null, UserStatus status = UserStatus.Active)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Phone = phone;
        Status = status;
    }

    public static User Create(string firstName, string lastName, string email, string passwordHash, string? phone = null)
    {
        return new User(Guid.NewGuid(), firstName, lastName, email, passwordHash, phone, UserStatus.Active);
    }

    public void UpdateProfile(string firstName, string lastName, string email, string? phone = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void SetStatus(UserStatus status)
    {
        Status = status;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AddRole(Guid roleId)
    {
        if (!_userRoles.Any(ur => ur.RoleId == roleId))
        {
            _userRoles.Add(new UserRole(Id, roleId));
            UpdateTimestamp(DateTime.UtcNow);
        }
    }

    public void RemoveRole(Guid roleId)
    {
        _userRoles.RemoveAll(ur => ur.RoleId == roleId);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AddRefreshToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
    }

    public void RevokeRefreshToken(string tokenHash)
    {
        var token = _refreshTokens.FirstOrDefault(rt => rt.TokenHash == tokenHash);
        token?.Revoke();
    }
}
