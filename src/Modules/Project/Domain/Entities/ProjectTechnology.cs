using BuildingBlocks.Domain.Models;

namespace Modules.Project.Domain.Entities;

public class ProjectTechnology : Entity
{
    public Guid ProjectId { get; private set; }
    public Guid TechnologyId { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;

    public Technology? Technology { get; private set; }

    private ProjectTechnology() { }

    public ProjectTechnology(Guid projectId, Guid technologyId, string version, string notes) : base(Guid.NewGuid())
    {
        ProjectId = projectId;
        TechnologyId = technologyId;
        Version = version;
        Notes = notes;
    }
}
