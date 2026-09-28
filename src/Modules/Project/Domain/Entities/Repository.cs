using BuildingBlocks.Domain.Models;

namespace Modules.Project.Domain.Entities;

public class Repository : Entity
{
    public Guid ProjectId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string Branch { get; private set; } = string.Empty;

    private Repository() { }

    public Repository(Guid id, Guid projectId, string type, string url, string branch) : base(id)
    {
        ProjectId = projectId;
        Type = type;
        Url = url;
        Branch = branch;
    }

    public static Repository Create(Guid projectId, string type, string url, string branch)
    {
        return new Repository(Guid.NewGuid(), projectId, type, url, branch);
    }
}
