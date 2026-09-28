using BuildingBlocks.Domain.Models;

namespace Modules.Project.Domain.Entities;

public class Project : AuditableAggregateRoot, ISoftDelete
{
    private readonly List<ProjectTechnology> _projectTechnologies = new();
    private readonly List<Repository> _repositories = new();
    private readonly List<Deployment> _deployments = new();

    public Guid CustomerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateOnly? StartDate { get; private set; }
    public DateOnly? DeliveryDate { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public ProjectRequirement? Requirement { get; private set; }
    public IReadOnlyCollection<ProjectTechnology> ProjectTechnologies => _projectTechnologies.AsReadOnly();
    public IReadOnlyCollection<Repository> Repositories => _repositories.AsReadOnly();
    public IReadOnlyCollection<Deployment> Deployments => _deployments.AsReadOnly();

    private Project() { }

    public Project(Guid id, Guid customerId, string name, string type, string status, DateOnly? startDate = null, DateOnly? deliveryDate = null) : base(id)
    {
        CustomerId = customerId;
        Name = name;
        Type = type;
        Status = status;
        StartDate = startDate;
        DeliveryDate = deliveryDate;
    }

    public static Project Create(Guid customerId, string name, string type, string status = "PLANNED", DateOnly? startDate = null, DateOnly? deliveryDate = null)
    {
        return new Project(Guid.NewGuid(), customerId, name, type, status, startDate, deliveryDate);
    }

    public void SetRequirement(ProjectRequirement requirement)
    {
        Requirement = requirement;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AddTechnology(Guid technologyId, string version, string notes)
    {
        if (!_projectTechnologies.Any(pt => pt.TechnologyId == technologyId))
        {
            _projectTechnologies.Add(new ProjectTechnology(Id, technologyId, version, notes));
            UpdateTimestamp(DateTime.UtcNow);
        }
    }

    public void AddRepository(Repository repository)
    {
        _repositories.Add(repository);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AddDeployment(Deployment deployment)
    {
        _deployments.Add(deployment);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
