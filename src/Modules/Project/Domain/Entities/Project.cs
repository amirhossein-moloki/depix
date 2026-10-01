using BuildingBlocks.Domain.Models;
using Modules.Project.Domain.Events;

namespace Modules.Project.Domain.Entities;

public class Project : AuditableAggregateRoot, ISoftDelete
{
    public static class Statuses
    {
        public const string Planned = "PLANNED";
        public const string InProgress = "IN_PROGRESS";
        public const string OnHold = "ON_HOLD";
        public const string Completed = "COMPLETED";
        public const string Cancelled = "CANCELLED";
        public const string Archived = "ARCHIVED";

        public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
        {
            Planned, InProgress, OnHold, Completed, Cancelled, Archived
        };
    }

    private readonly List<ProjectTechnology> _projectTechnologies = new();
    private readonly List<Repository> _repositories = new();
    private readonly List<Deployment> _deployments = new();

    public Guid CustomerId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? ContactId { get; private set; }
    public Guid? OpportunityId { get; private set; }
    public Guid? ProposalId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;

    public DateOnly? StartDate { get; private set; }
    public DateOnly? PlannedDeliveryDate { get; private set; }
    public DateOnly? ActualDeliveryDate { get; private set; }
    public string? Notes { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public ProjectRequirement? Requirement { get; private set; }
    public IReadOnlyCollection<ProjectTechnology> ProjectTechnologies => _projectTechnologies.AsReadOnly();
    public IReadOnlyCollection<Repository> Repositories => _repositories.AsReadOnly();
    public IReadOnlyCollection<Deployment> Deployments => _deployments.AsReadOnly();

    private Project() { }

    public Project(
        Guid id,
        Guid customerId,
        string name,
        string? type = null,
        string status = Statuses.Planned,
        Guid? companyId = null,
        Guid? contactId = null,
        Guid? opportunityId = null,
        Guid? proposalId = null,
        string? description = null,
        DateOnly? startDate = null,
        DateOnly? plannedDeliveryDate = null,
        DateOnly? actualDeliveryDate = null,
        string? notes = null) : base(id)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("Customer ID is required.", nameof(customerId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        CustomerId = customerId;
        Name = name.Trim();
        Type = type?.Trim() ?? "WebDevelopment";
        Status = string.IsNullOrWhiteSpace(status) ? Statuses.Planned : status.ToUpperInvariant();
        CompanyId = companyId;
        ContactId = contactId;
        OpportunityId = opportunityId;
        ProposalId = proposalId;
        Description = description?.Trim();
        StartDate = startDate;
        PlannedDeliveryDate = plannedDeliveryDate;
        ActualDeliveryDate = actualDeliveryDate;
        Notes = notes?.Trim();
    }

    public static Project Create(
        Guid customerId,
        string name,
        string? type = null,
        Guid? companyId = null,
        Guid? contactId = null,
        Guid? opportunityId = null,
        Guid? proposalId = null,
        string? description = null,
        DateOnly? startDate = null,
        DateOnly? plannedDeliveryDate = null,
        string? notes = null)
    {
        var project = new Project(
            Guid.NewGuid(),
            customerId,
            name,
            type,
            Statuses.Planned,
            companyId,
            contactId,
            opportunityId,
            proposalId,
            description,
            startDate,
            plannedDeliveryDate,
            actualDeliveryDate: null,
            notes);

        project.AddDomainEvent(new ProjectCreatedEvent(project.Id, project.CustomerId, project.Name));
        return project;
    }

    public void UpdateDetails(
        string name,
        string? type,
        string? description,
        DateOnly? startDate,
        DateOnly? plannedDeliveryDate,
        DateOnly? actualDeliveryDate,
        Guid? companyId,
        Guid? contactId,
        Guid? opportunityId,
        Guid? proposalId,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Project name is required.", nameof(name));
        }

        Name = name.Trim();
        Type = type?.Trim() ?? Type;
        Description = description?.Trim();
        StartDate = startDate;
        PlannedDeliveryDate = plannedDeliveryDate;
        ActualDeliveryDate = actualDeliveryDate;
        CompanyId = companyId;
        ContactId = contactId;
        OpportunityId = opportunityId;
        ProposalId = proposalId;
        Notes = notes?.Trim();

        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UpdateStatus(string newStatus)
    {
        if (string.IsNullOrWhiteSpace(newStatus) || !Statuses.All.Contains(newStatus))
        {
            throw new ArgumentException($"Invalid status '{newStatus}'. Allowed statuses: {string.Join(", ", Statuses.All)}.", nameof(newStatus));
        }

        var normalizedStatus = newStatus.ToUpperInvariant();
        if (Status == normalizedStatus)
        {
            return;
        }

        if (Status == Statuses.Cancelled && normalizedStatus != Statuses.Planned && normalizedStatus != Statuses.Archived)
        {
            throw new InvalidOperationException("Cancelled projects cannot transition directly to active states without reactivation.");
        }

        var oldStatus = Status;
        Status = normalizedStatus;
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new ProjectStatusChangedEvent(Id, oldStatus, Status));
    }

    public void Start(DateOnly? startDate = null)
    {
        if (Status == Statuses.Cancelled || Status == Statuses.Completed || Status == Statuses.Archived)
        {
            throw new InvalidOperationException($"Cannot start project in '{Status}' state.");
        }

        var oldStatus = Status;
        Status = Statuses.InProgress;
        StartDate = startDate ?? StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new ProjectStartedEvent(Id, StartDate.Value));
        if (oldStatus != Statuses.InProgress)
        {
            AddDomainEvent(new ProjectStatusChangedEvent(Id, oldStatus, Statuses.InProgress));
        }
    }

    public void PutOnHold()
    {
        if (Status == Statuses.Cancelled || Status == Statuses.Completed || Status == Statuses.Archived)
        {
            throw new InvalidOperationException($"Cannot put project on hold in '{Status}' state.");
        }

        UpdateStatus(Statuses.OnHold);
    }

    public void Resume()
    {
        if (Status != Statuses.OnHold)
        {
            throw new InvalidOperationException($"Only projects in '{Statuses.OnHold}' status can be resumed.");
        }

        UpdateStatus(Statuses.InProgress);
    }

    public void Complete(DateOnly? actualDeliveryDate = null)
    {
        if (Status == Statuses.Cancelled || Status == Statuses.Archived)
        {
            throw new InvalidOperationException($"Cannot complete project in '{Status}' state.");
        }

        var oldStatus = Status;
        Status = Statuses.Completed;
        ActualDeliveryDate = actualDeliveryDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new ProjectCompletedEvent(Id, ActualDeliveryDate.Value));
        if (oldStatus != Statuses.Completed)
        {
            AddDomainEvent(new ProjectStatusChangedEvent(Id, oldStatus, Statuses.Completed));
        }
    }

    public void Cancel(string? reason = null)
    {
        if (Status == Statuses.Archived)
        {
            throw new InvalidOperationException("Cannot cancel an archived project.");
        }

        var oldStatus = Status;
        Status = Statuses.Cancelled;
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new ProjectCancelledEvent(Id, reason));
        if (oldStatus != Statuses.Cancelled)
        {
            AddDomainEvent(new ProjectStatusChangedEvent(Id, oldStatus, Statuses.Cancelled));
        }
    }

    public void SetRequirement(string? businessGoal, string? features, string? technologyNotes, string? targetAudience)
    {
        if (Requirement == null)
        {
            Requirement = ProjectRequirement.Create(Id, businessGoal, features, technologyNotes, targetAudience);
        }
        else
        {
            Requirement.Update(businessGoal, features, technologyNotes, targetAudience);
        }
        UpdateTimestamp(DateTime.UtcNow);
    }

    public Repository AddRepository(string type, string url, string? name = null, string? branch = null, string? description = null)
    {
        var repository = Repository.Create(Id, type, url, name, branch, description);
        _repositories.Add(repository);
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new RepositoryAddedEvent(Id, repository.Id, repository.Url));
        return repository;
    }

    public void UpdateRepository(Guid repositoryId, string type, string url, string? name = null, string? branch = null, string? description = null)
    {
        var repository = _repositories.FirstOrDefault(r => r.Id == repositoryId);
        if (repository == null)
        {
            throw new KeyNotFoundException($"Repository with ID '{repositoryId}' was not found on this project.");
        }

        repository.Update(type, url, name, branch, description);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void RemoveRepository(Guid repositoryId)
    {
        var repository = _repositories.FirstOrDefault(r => r.Id == repositoryId);
        if (repository != null)
        {
            _repositories.Remove(repository);
            UpdateTimestamp(DateTime.UtcNow);
        }
    }

    public Deployment AddDeployment(
        string environment,
        string? server = null,
        string? provider = null,
        string? domain = null,
        string? sslStatus = null,
        string? version = null,
        string? notes = null)
    {
        var deployment = Deployment.Create(Id, environment, server, provider, domain, sslStatus, version, notes);
        _deployments.Add(deployment);
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new DeploymentRecordedEvent(Id, deployment.Id, deployment.Environment, deployment.Domain));
        return deployment;
    }

    public void UpdateDeployment(
        Guid deploymentId,
        string environment,
        string? server = null,
        string? provider = null,
        string? domain = null,
        string? sslStatus = null,
        string? version = null,
        string? notes = null)
    {
        var deployment = _deployments.FirstOrDefault(d => d.Id == deploymentId);
        if (deployment == null)
        {
            throw new KeyNotFoundException($"Deployment with ID '{deploymentId}' was not found on this project.");
        }

        deployment.Update(environment, server, provider, domain, sslStatus, version, notes);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void RemoveDeployment(Guid deploymentId)
    {
        var deployment = _deployments.FirstOrDefault(d => d.Id == deploymentId);
        if (deployment != null)
        {
            _deployments.Remove(deployment);
            UpdateTimestamp(DateTime.UtcNow);
        }
    }

    public void AddTechnology(Guid technologyId, string? version = null, string? notes = null)
    {
        var existing = _projectTechnologies.FirstOrDefault(pt => pt.TechnologyId == technologyId);
        if (existing == null)
        {
            _projectTechnologies.Add(new ProjectTechnology(Id, technologyId, version, notes));
        }
        else
        {
            existing.Update(version, notes);
        }
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void RemoveTechnology(Guid technologyId)
    {
        _projectTechnologies.RemoveAll(pt => pt.TechnologyId == technologyId);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void Archive(Guid? archivedBy = null)
    {
        SoftDelete(archivedBy);
    }

    public void Reactivate()
    {
        UndoSoftDelete();
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        var oldStatus = Status;
        Status = Statuses.Archived;
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new ProjectStatusChangedEvent(Id, oldStatus, Statuses.Archived));
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        var oldStatus = Status;
        Status = Statuses.Planned;
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new ProjectStatusChangedEvent(Id, oldStatus, Statuses.Planned));
    }
}
