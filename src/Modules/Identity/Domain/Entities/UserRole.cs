using BuildingBlocks.Domain.Models;

namespace Modules.Identity.Domain.Entities;

public class UserRole : Entity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    private UserRole() { }

    public UserRole(Guid userId, Guid roleId) : base(Guid.NewGuid())
    {
        UserId = userId;
        RoleId = roleId;
    }
}
