using FluentValidation;
using Modules.Project.Application.Commands;
using ProjectEntity = Modules.Project.Domain.Entities.Project;

namespace Modules.Project.Application.Validators;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters.");

        RuleFor(x => x.Type)
            .MaximumLength(50).WithMessage("Type cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");
    }
}

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters.");

        RuleFor(x => x.Type)
            .MaximumLength(50).WithMessage("Type cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");
    }
}

public class ChangeProjectStatusCommandValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => ProjectEntity.Statuses.All.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ProjectEntity.Statuses.All)}");
    }
}

public class AddRepositoryCommandValidator : AbstractValidator<AddRepositoryCommand>
{
    public AddRepositoryCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Repository URL is required.")
            .MaximumLength(500).WithMessage("Repository URL cannot exceed 500 characters.");

        RuleFor(x => x.Type)
            .MaximumLength(50).WithMessage("Type cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Branch)
            .MaximumLength(100).WithMessage("Branch cannot exceed 100 characters.");
    }
}

public class UpdateRepositoryCommandValidator : AbstractValidator<UpdateRepositoryCommand>
{
    public UpdateRepositoryCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.RepositoryId)
            .NotEmpty().WithMessage("RepositoryId is required.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Repository URL is required.")
            .MaximumLength(500).WithMessage("Repository URL cannot exceed 500 characters.");

        RuleFor(x => x.Type)
            .MaximumLength(50).WithMessage("Type cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Branch)
            .MaximumLength(100).WithMessage("Branch cannot exceed 100 characters.");
    }
}

public class AddDeploymentCommandValidator : AbstractValidator<AddDeploymentCommand>
{
    public AddDeploymentCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.Environment)
            .NotEmpty().WithMessage("Deployment Environment is required.")
            .MaximumLength(50).WithMessage("Environment cannot exceed 50 characters.");

        RuleFor(x => x.Domain)
            .MaximumLength(200).WithMessage("Domain cannot exceed 200 characters.");

        RuleFor(x => x.Server)
            .MaximumLength(100).WithMessage("Server cannot exceed 100 characters.");

        RuleFor(x => x.Provider)
            .MaximumLength(100).WithMessage("Provider cannot exceed 100 characters.");
    }
}

public class UpdateDeploymentCommandValidator : AbstractValidator<UpdateDeploymentCommand>
{
    public UpdateDeploymentCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.");

        RuleFor(x => x.DeploymentId)
            .NotEmpty().WithMessage("DeploymentId is required.");

        RuleFor(x => x.Environment)
            .NotEmpty().WithMessage("Deployment Environment is required.")
            .MaximumLength(50).WithMessage("Environment cannot exceed 50 characters.");

        RuleFor(x => x.Domain)
            .MaximumLength(200).WithMessage("Domain cannot exceed 200 characters.");

        RuleFor(x => x.Server)
            .MaximumLength(100).WithMessage("Server cannot exceed 100 characters.");

        RuleFor(x => x.Provider)
            .MaximumLength(100).WithMessage("Provider cannot exceed 100 characters.");
    }
}
