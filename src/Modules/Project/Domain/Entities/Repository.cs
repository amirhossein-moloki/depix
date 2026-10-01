using BuildingBlocks.Domain.Models;

namespace Modules.Project.Domain.Entities;

public class Repository : Entity
{
    public Guid ProjectId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Branch { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private Repository() { }

    public Repository(Guid id, Guid projectId, string type, string url, string name, string branch, string description) : base(id)
    {
        ProjectId = projectId;
        Type = type;
        Url = url;
        Name = name;
        Branch = branch;
        Description = description;
    }

    public static Repository Create(Guid projectId, string type, string url, string? name = null, string? branch = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Repository URL cannot be empty.", nameof(url));
        }

        return new Repository(
            Guid.NewGuid(),
            projectId,
            type ?? "GitHub",
            url.Trim(),
            name?.Trim() ?? string.Empty,
            branch?.Trim() ?? "main",
            description?.Trim() ?? string.Empty);
    }

    public void Update(string type, string url, string? name, string? branch, string? description)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Repository URL cannot be empty.", nameof(url));
        }

        Type = type ?? "GitHub";
        Url = url.Trim();
        Name = name?.Trim() ?? string.Empty;
        Branch = branch?.Trim() ?? "main";
        Description = description?.Trim() ?? string.Empty;
    }
}
