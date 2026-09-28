using BuildingBlocks.Domain.Models;

namespace Modules.Identity.Domain.Entities;

public class RolePermission : Entity
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId) : base(Guid.NewGuid())
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }
}
