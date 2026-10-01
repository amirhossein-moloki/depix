using BuildingBlocks.Application.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.Project.Application.Commands;
using Modules.Project.Application.DTOs;
using Modules.Project.Application.Queries;
using Modules.Project.Application.Validators;

namespace Modules.Project.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectApplication(this IServiceCollection services)
    {
        // Command Handlers
        services.AddScoped<ICommandHandler<CreateProjectCommand, ProjectDetailDto>, CreateProjectCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProjectCommand, ProjectDetailDto>, UpdateProjectCommandHandler>();
        services.AddScoped<ICommandHandler<StartProjectCommand, ProjectDetailDto>, StartProjectCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeProjectStatusCommand, ProjectDetailDto>, ChangeProjectStatusCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteProjectCommand, ProjectDetailDto>, CompleteProjectCommandHandler>();
        services.AddScoped<ICommandHandler<CancelProjectCommand, ProjectDetailDto>, CancelProjectCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveProjectCommand>, ArchiveProjectCommandHandler>();

        services.AddScoped<ICommandHandler<AddRepositoryCommand, RepositoryDto>, AddRepositoryCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateRepositoryCommand, RepositoryDto>, UpdateRepositoryCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveRepositoryCommand>, RemoveRepositoryCommandHandler>();

        services.AddScoped<ICommandHandler<AddDeploymentCommand, DeploymentDto>, AddDeploymentCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateDeploymentCommand, DeploymentDto>, UpdateDeploymentCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveDeploymentCommand>, RemoveDeploymentCommandHandler>();

        services.AddScoped<ICommandHandler<SetRequirementCommand, ProjectRequirementDto>, SetRequirementCommandHandler>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetProjectByIdQuery, ProjectDetailDto>, GetProjectByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetProjectsQuery, PagedResult<ProjectListItemDto>>, GetProjectsQueryHandler>();

        // Validators
        services.AddScoped<IValidator<CreateProjectCommand>, CreateProjectCommandValidator>();
        services.AddScoped<IValidator<UpdateProjectCommand>, UpdateProjectCommandValidator>();
        services.AddScoped<IValidator<ChangeProjectStatusCommand>, ChangeProjectStatusCommandValidator>();
        services.AddScoped<IValidator<AddRepositoryCommand>, AddRepositoryCommandValidator>();
        services.AddScoped<IValidator<UpdateRepositoryCommand>, UpdateRepositoryCommandValidator>();
        services.AddScoped<IValidator<AddDeploymentCommand>, AddDeploymentCommandValidator>();
        services.AddScoped<IValidator<UpdateDeploymentCommand>, UpdateDeploymentCommandValidator>();

        return services;
    }
}
