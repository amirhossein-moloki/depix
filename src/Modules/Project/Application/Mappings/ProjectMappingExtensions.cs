using Modules.Project.Application.DTOs;
using ProjectEntity = Modules.Project.Domain.Entities.Project;
using Modules.Project.Domain.Entities;

namespace Modules.Project.Application.Mappings;

public static class ProjectMappingExtensions
{
    public static ProjectDto ToDto(this ProjectEntity project)
    {
        return new ProjectDto(
            project.Id,
            project.CustomerId,
            project.CompanyId,
            project.ContactId,
            project.OpportunityId,
            project.ProposalId,
            project.Name,
            project.Type,
            project.Status,
            project.Description,
            project.StartDate,
            project.PlannedDeliveryDate,
            project.ActualDeliveryDate,
            project.Notes,
            project.IsDeleted,
            project.CreatedAt,
            project.UpdatedAt);
    }

    public static RepositoryDto ToDto(this Repository repository)
    {
        return new RepositoryDto(
            repository.Id,
            repository.ProjectId,
            repository.Type,
            repository.Url,
            repository.Name,
            repository.Branch,
            repository.Description);
    }

    public static DeploymentDto ToDto(this Deployment deployment)
    {
        return new DeploymentDto(
            deployment.Id,
            deployment.ProjectId,
            deployment.Environment,
            deployment.Server,
            deployment.Provider,
            deployment.Domain,
            deployment.SslStatus,
            deployment.Version,
            deployment.DeployedAt,
            deployment.Notes);
    }

    public static ProjectTechnologyDto ToDto(this ProjectTechnology pt)
    {
        return new ProjectTechnologyDto(
            pt.Id,
            pt.ProjectId,
            pt.TechnologyId,
            pt.Technology?.Name,
            pt.Technology?.Category,
            pt.Version,
            pt.Notes);
    }

    public static ProjectRequirementDto ToDto(this ProjectRequirement requirement)
    {
        return new ProjectRequirementDto(
            requirement.Id,
            requirement.ProjectId,
            requirement.BusinessGoal,
            requirement.Features,
            requirement.TechnologyNotes,
            requirement.TargetAudience);
    }

    public static ProjectDetailDto ToDetailDto(this ProjectEntity project)
    {
        return new ProjectDetailDto(
            project.Id,
            project.CustomerId,
            project.CompanyId,
            project.ContactId,
            project.OpportunityId,
            project.ProposalId,
            project.Name,
            project.Type,
            project.Status,
            project.Description,
            project.StartDate,
            project.PlannedDeliveryDate,
            project.ActualDeliveryDate,
            project.Notes,
            project.IsDeleted,
            project.CreatedAt,
            project.UpdatedAt,
            project.Requirement?.ToDto(),
            project.Repositories.Select(r => r.ToDto()).ToList(),
            project.Deployments.Select(d => d.ToDto()).ToList(),
            project.ProjectTechnologies.Select(pt => pt.ToDto()).ToList());
    }

    public static ProjectListItemDto ToListItemDto(this ProjectEntity project)
    {
        return new ProjectListItemDto(
            project.Id,
            project.CustomerId,
            project.CompanyId,
            project.Name,
            project.Type,
            project.Status,
            project.StartDate,
            project.PlannedDeliveryDate,
            project.ActualDeliveryDate,
            project.Repositories.Count,
            project.Deployments.Count,
            project.CreatedAt);
    }
}
