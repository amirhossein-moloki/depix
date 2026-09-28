using BuildingBlocks.Domain.Models;

namespace Modules.Project.Domain.Entities;

public class Technology : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Vendor { get; private set; } = string.Empty;
    public string Website { get; private set; } = string.Empty;

    private Technology() { }

    public Technology(Guid id, string name, string category, string vendor, string website) : base(id)
    {
        Name = name;
        Category = category;
        Vendor = vendor;
        Website = website;
    }

    public static Technology Create(string name, string category, string vendor, string website)
    {
        return new Technology(Guid.NewGuid(), name, category, vendor, website);
    }
}
