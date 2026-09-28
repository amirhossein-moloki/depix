using BuildingBlocks.Domain.Models;

namespace Modules.Identity.Domain.Entities;

public class Role : Entity
{
    public string Name { get; private set; } = string.Empty;

    private Role() { }

    public Role(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static Role Create(string name)
    {
        return new Role(Guid.NewGuid(), name);
    }
}
