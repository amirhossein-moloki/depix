using BuildingBlocks.Domain.Models;

namespace Modules.Identity.Domain.Entities;

public class Permission : Entity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;

    private Permission() { }

    public Permission(Guid id, string code, string name, string module) : base(id)
    {
        Code = code;
        Name = name;
        Module = module;
    }

    public static Permission Create(string code, string name, string module)
    {
        return new Permission(Guid.NewGuid(), code, name, module);
    }
}
