using BuildingBlocks.Domain.Models;

namespace Modules.Project.Domain.Entities;

public class ProjectRequirement : Entity
{
    public Guid ProjectId { get; private set; }
    public string BusinessGoal { get; private set; } = string.Empty;
    public string Features { get; private set; } = string.Empty;
    public string TechnologyNotes { get; private set; } = string.Empty;
    public string TargetAudience { get; private set; } = string.Empty;

    private ProjectRequirement() { }

    public ProjectRequirement(Guid id, Guid projectId, string businessGoal, string features, string technologyNotes, string targetAudience) : base(id)
    {
        ProjectId = projectId;
        BusinessGoal = businessGoal;
        Features = features;
        TechnologyNotes = technologyNotes;
        TargetAudience = targetAudience;
    }

    public static ProjectRequirement Create(Guid projectId, string? businessGoal, string? features, string? technologyNotes, string? targetAudience)
    {
        return new ProjectRequirement(
            Guid.NewGuid(),
            projectId,
            businessGoal?.Trim() ?? string.Empty,
            features?.Trim() ?? string.Empty,
            technologyNotes?.Trim() ?? string.Empty,
            targetAudience?.Trim() ?? string.Empty);
    }

    public void Update(string? businessGoal, string? features, string? technologyNotes, string? targetAudience)
    {
        BusinessGoal = businessGoal?.Trim() ?? string.Empty;
        Features = features?.Trim() ?? string.Empty;
        TechnologyNotes = technologyNotes?.Trim() ?? string.Empty;
        TargetAudience = targetAudience?.Trim() ?? string.Empty;
    }
}
