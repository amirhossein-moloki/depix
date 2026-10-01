namespace Modules.Project.Application.DTOs;

public record ProjectDto(
    Guid Id,
    Guid CustomerId,
    Guid? CompanyId,
    Guid? ContactId,
    Guid? OpportunityId,
    Guid? ProposalId,
    string Name,
    string Type,
    string Status,
    string? Description,
    DateOnly? StartDate,
    DateOnly? PlannedDeliveryDate,
    DateOnly? ActualDeliveryDate,
    string? Notes,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record RepositoryDto(
    Guid Id,
    Guid ProjectId,
    string Type,
    string Url,
    string Name,
    string Branch,
    string Description);

public record DeploymentDto(
    Guid Id,
    Guid ProjectId,
    string Environment,
    string Server,
    string Provider,
    string Domain,
    string SslStatus,
    string Version,
    DateTime DeployedAt,
    string Notes);

public record ProjectTechnologyDto(
    Guid Id,
    Guid ProjectId,
    Guid TechnologyId,
    string? TechnologyName,
    string? Category,
    string Version,
    string Notes);

public record ProjectRequirementDto(
    Guid Id,
    Guid ProjectId,
    string BusinessGoal,
    string Features,
    string TechnologyNotes,
    string TargetAudience);

public record ProjectDetailDto(
    Guid Id,
    Guid CustomerId,
    Guid? CompanyId,
    Guid? ContactId,
    Guid? OpportunityId,
    Guid? ProposalId,
    string Name,
    string Type,
    string Status,
    string? Description,
    DateOnly? StartDate,
    DateOnly? PlannedDeliveryDate,
    DateOnly? ActualDeliveryDate,
    string? Notes,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    ProjectRequirementDto? Requirement,
    List<RepositoryDto> Repositories,
    List<DeploymentDto> Deployments,
    List<ProjectTechnologyDto> Technologies);

public record ProjectListItemDto(
    Guid Id,
    Guid CustomerId,
    Guid? CompanyId,
    string Name,
    string Type,
    string Status,
    DateOnly? StartDate,
    DateOnly? PlannedDeliveryDate,
    DateOnly? ActualDeliveryDate,
    int RepositoriesCount,
    int DeploymentsCount,
    DateTime CreatedAt);

public record CreateProjectRequest(
    Guid CustomerId,
    string Name,
    string? Type = null,
    Guid? CompanyId = null,
    Guid? ContactId = null,
    Guid? OpportunityId = null,
    Guid? ProposalId = null,
    string? Description = null,
    DateOnly? StartDate = null,
    DateOnly? PlannedDeliveryDate = null,
    string? Notes = null);

public record UpdateProjectRequest(
    string Name,
    string? Type = null,
    string? Description = null,
    DateOnly? StartDate = null,
    DateOnly? PlannedDeliveryDate = null,
    DateOnly? ActualDeliveryDate = null,
    Guid? CompanyId = null,
    Guid? ContactId = null,
    Guid? OpportunityId = null,
    Guid? ProposalId = null,
    string? Notes = null);

public record ChangeProjectStatusRequest(string Status);
public record StartProjectRequest(DateOnly? StartDate = null);
public record CompleteProjectRequest(DateOnly? ActualDeliveryDate = null);
public record CancelProjectRequest(string? Reason = null);

public record AddRepositoryRequest(
    string Type,
    string Url,
    string? Name = null,
    string? Branch = null,
    string? Description = null);

public record UpdateRepositoryRequest(
    string Type,
    string Url,
    string? Name = null,
    string? Branch = null,
    string? Description = null);

public record AddDeploymentRequest(
    string Environment,
    string? Server = null,
    string? Provider = null,
    string? Domain = null,
    string? SslStatus = null,
    string? Version = null,
    string? Notes = null);

public record UpdateDeploymentRequest(
    string Environment,
    string? Server = null,
    string? Provider = null,
    string? Domain = null,
    string? SslStatus = null,
    string? Version = null,
    string? Notes = null);

public record SetRequirementRequest(
    string? BusinessGoal = null,
    string? Features = null,
    string? TechnologyNotes = null,
    string? TargetAudience = null);
