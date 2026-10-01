using BuildingBlocks.Domain.Events;

namespace Modules.Project.Domain.Events;

public record ProjectCreatedEvent(
    Guid ProjectId,
    Guid CustomerId,
    string Name) : DomainEvent;

public record ProjectStartedEvent(
    Guid ProjectId,
    DateOnly StartDate) : DomainEvent;

public record ProjectStatusChangedEvent(
    Guid ProjectId,
    string OldStatus,
    string NewStatus) : DomainEvent;

public record ProjectCompletedEvent(
    Guid ProjectId,
    DateOnly ActualDeliveryDate) : DomainEvent;

public record ProjectCancelledEvent(
    Guid ProjectId,
    string? Reason) : DomainEvent;

public record RepositoryAddedEvent(
    Guid ProjectId,
    Guid RepositoryId,
    string Url) : DomainEvent;

public record DeploymentRecordedEvent(
    Guid ProjectId,
    Guid DeploymentId,
    string Environment,
    string Domain) : DomainEvent;
