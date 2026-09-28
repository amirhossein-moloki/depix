using BuildingBlocks.Domain.Models;

namespace Modules.Identity.Domain.Entities;

public class Role : Entity
{
    private readonly List<RolePermission> _rolePermissions = new();

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { }

    public Role(Guid id, string name, string? description = null) : base(id)
    {
        Name = name;
        Description = description;
    }

    public static Role Create(string name, string? description = null)
    {
        return new Role(Guid.NewGuid(), name, description);
    }

    public void AddPermission(Guid permissionId)
    {
        if (!_rolePermissions.Any(rp => rp.PermissionId == permissionId))
        {
            _rolePermissions.Add(new RolePermission(Id, permissionId));
        }
    }

    public void RemovePermission(Guid permissionId)
    {
        _rolePermissions.RemoveAll(rp => rp.PermissionId == permissionId);
    }
}
