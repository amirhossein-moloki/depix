using Xunit;
using Modules.Project.Domain.Entities;
using Modules.Project.Domain.Events;

namespace UnitTests;

public class ProjectDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializeProjectAndRaiseEvents()
    {
        var customerId = Guid.NewGuid();
        var name = "E-Commerce Web Portal";
        var companyId = Guid.NewGuid();

        var project = Project.Create(
            customerId,
            name,
            type: "WebDevelopment",
            companyId: companyId,
            description: "Custom web platform",
            startDate: new DateOnly(2026, 10, 1),
            plannedDeliveryDate: new DateOnly(2026, 12, 31),
            notes: "Phase 8 project");

        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.Equal(customerId, project.CustomerId);
        Assert.Equal("E-Commerce Web Portal", project.Name);
        Assert.Equal("PLANNED", project.Status);
        Assert.Equal(companyId, project.CompanyId);
        Assert.Equal("Custom web platform", project.Description);
        Assert.Equal(new DateOnly(2026, 10, 1), project.StartDate);
        Assert.Equal(new DateOnly(2026, 12, 31), project.PlannedDeliveryDate);
        Assert.Null(project.ActualDeliveryDate);
        Assert.False(project.IsDeleted);

        Assert.Contains(project.DomainEvents, e => e is ProjectCreatedEvent p && p.ProjectId == project.Id);
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Project.Create(Guid.Empty, "Web Portal"));
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Project.Create(Guid.NewGuid(), "   "));
    }

    [Fact]
    public void Start_ShouldChangeStatusToInProgressAndSetStartDate()
    {
        var project = Project.Create(Guid.NewGuid(), "ERP Dashboard");
        project.ClearDomainEvents();

        var startDate = new DateOnly(2026, 10, 15);
        project.Start(startDate);

        Assert.Equal("IN_PROGRESS", project.Status);
        Assert.Equal(startDate, project.StartDate);
        Assert.Contains(project.DomainEvents, e => e is ProjectStartedEvent s && s.StartDate == startDate);
    }

    [Fact]
    public void PutOnHold_And_Resume_ShouldTransitionStatuses()
    {
        var project = Project.Create(Guid.NewGuid(), "SaaS Mobile Backend");
        project.Start();
        project.ClearDomainEvents();

        project.PutOnHold();
        Assert.Equal("ON_HOLD", project.Status);

        project.Resume();
        Assert.Equal("IN_PROGRESS", project.Status);
    }

    [Fact]
    public void Complete_ShouldSetStatusToCompletedAndSetActualDeliveryDate()
    {
        var project = Project.Create(Guid.NewGuid(), "CRM Refactoring");
        project.Start();
        project.ClearDomainEvents();

        var deliveryDate = new DateOnly(2026, 11, 30);
        project.Complete(deliveryDate);

        Assert.Equal("COMPLETED", project.Status);
        Assert.Equal(deliveryDate, project.ActualDeliveryDate);
        Assert.Contains(project.DomainEvents, e => e is ProjectCompletedEvent c && c.ActualDeliveryDate == deliveryDate);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelledAndRaiseEvent()
    {
        var project = Project.Create(Guid.NewGuid(), "Legacy Migration");
        project.ClearDomainEvents();

        project.Cancel("Client budget cut");

        Assert.Equal("CANCELLED", project.Status);
        Assert.Contains(project.DomainEvents, e => e is ProjectCancelledEvent c && c.Reason == "Client budget cut");
    }

    [Fact]
    public void CancelledProject_DirectStartOrComplete_ShouldThrowInvalidOperationException()
    {
        var project = Project.Create(Guid.NewGuid(), "Legacy Migration");
        project.Cancel();

        Assert.Throws<InvalidOperationException>(() => project.Start());
        Assert.Throws<InvalidOperationException>(() => project.Complete());
    }

    [Fact]
    public void AddRepository_ShouldAddRepositoryAndRaiseEvent()
    {
        var project = Project.Create(Guid.NewGuid(), "Client Portal");
        project.ClearDomainEvents();

        var repo = project.AddRepository("GitHub", "https://github.com/company/portal.git", "Portal App", "main", "Main web app repo");

        Assert.Single(project.Repositories);
        Assert.Equal("https://github.com/company/portal.git", repo.Url);
        Assert.Equal("Portal App", repo.Name);
        Assert.Contains(project.DomainEvents, e => e is RepositoryAddedEvent r && r.RepositoryId == repo.Id);
    }

    [Fact]
    public void AddDeployment_ShouldAddDeploymentAndRaiseEvent()
    {
        var project = Project.Create(Guid.NewGuid(), "Client Portal");
        project.ClearDomainEvents();

        var deployment = project.AddDeployment("Production", "k8s-prod-1", "AWS", "portal.company.com", "Active", "1.0.0", "Prod deployment");

        Assert.Single(project.Deployments);
        Assert.Equal("Production", deployment.Environment);
        Assert.Equal("portal.company.com", deployment.Domain);
        Assert.Contains(project.DomainEvents, e => e is DeploymentRecordedEvent d && d.DeploymentId == deployment.Id);
    }

    [Fact]
    public void SoftDelete_And_Reactivate_ShouldManageStateAndStatus()
    {
        var project = Project.Create(Guid.NewGuid(), "Testing Soft Delete");
        var archivedBy = Guid.NewGuid();

        project.Archive(archivedBy);
        Assert.True(project.IsDeleted);
        Assert.Equal("ARCHIVED", project.Status);
        Assert.Equal(archivedBy, project.DeletedBy);
        Assert.NotNull(project.DeletedAt);

        project.Reactivate();
        Assert.False(project.IsDeleted);
        Assert.Equal("PLANNED", project.Status);
        Assert.Null(project.DeletedBy);
        Assert.Null(project.DeletedAt);
    }
}
