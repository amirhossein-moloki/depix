using BuildingBlocks.Domain.Models;

namespace Modules.Identity.Domain.Entities;

public class User : AuditableAggregateRoot
{
    private readonly List<UserRole> _userRoles = new();

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User() { }

    public User(Guid id, string name, string email, string passwordHash, bool isActive = true) : base(id)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        IsActive = isActive;
    }

    public static User Create(string name, string email, string passwordHash)
    {
        return new User(Guid.NewGuid(), name, email, passwordHash, true);
    }

    public void UpdateProfile(string name, string email)
    {
        Name = name;
        Email = email;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
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
}
