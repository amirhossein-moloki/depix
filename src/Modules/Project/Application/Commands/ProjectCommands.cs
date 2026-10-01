using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Project.Application.DTOs;
using Modules.Project.Application.Mappings;
using Modules.Project.Domain.Repositories;
using ProjectEntity = Modules.Project.Domain.Entities.Project;

namespace Modules.Project.Application.Commands;

// Commands
public record CreateProjectCommand(
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
    string? Notes = null) : ICommand<ProjectDetailDto>;

public record UpdateProjectCommand(
    Guid Id,
    string Name,
    string? Type,
    string? Description,
    DateOnly? StartDate,
    DateOnly? PlannedDeliveryDate,
    DateOnly? ActualDeliveryDate,
    Guid? CompanyId,
    Guid? ContactId,
    Guid? OpportunityId,
    Guid? ProposalId,
    string? Notes) : ICommand<ProjectDetailDto>;

public record StartProjectCommand(Guid Id, DateOnly? StartDate = null) : ICommand<ProjectDetailDto>;
public record ChangeProjectStatusCommand(Guid Id, string Status) : ICommand<ProjectDetailDto>;
public record CompleteProjectCommand(Guid Id, DateOnly? ActualDeliveryDate = null) : ICommand<ProjectDetailDto>;
public record CancelProjectCommand(Guid Id, string? Reason = null) : ICommand<ProjectDetailDto>;
public record ArchiveProjectCommand(Guid Id, Guid? ArchivedBy = null) : ICommand;

public record AddRepositoryCommand(
    Guid ProjectId,
    string Type,
    string Url,
    string? Name = null,
    string? Branch = null,
    string? Description = null) : ICommand<RepositoryDto>;

public record UpdateRepositoryCommand(
    Guid ProjectId,
    Guid RepositoryId,
    string Type,
    string Url,
    string? Name = null,
    string? Branch = null,
    string? Description = null) : ICommand<RepositoryDto>;

public record RemoveRepositoryCommand(Guid ProjectId, Guid RepositoryId) : ICommand;

public record AddDeploymentCommand(
    Guid ProjectId,
    string Environment,
    string? Server = null,
    string? Provider = null,
    string? Domain = null,
    string? SslStatus = null,
    string? Version = null,
    string? Notes = null) : ICommand<DeploymentDto>;

public record UpdateDeploymentCommand(
    Guid ProjectId,
    Guid DeploymentId,
    string Environment,
    string? Server = null,
    string? Provider = null,
    string? Domain = null,
    string? SslStatus = null,
    string? Version = null,
    string? Notes = null) : ICommand<DeploymentDto>;

public record RemoveDeploymentCommand(Guid ProjectId, Guid DeploymentId) : ICommand;

public record SetRequirementCommand(
    Guid ProjectId,
    string? BusinessGoal = null,
    string? Features = null,
    string? TechnologyNotes = null,
    string? TargetAudience = null) : ICommand<ProjectRequirementDto>;


// Handlers

public class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, ProjectDetailDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICustomerService _customerService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        ICustomerService customerService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _customerService = customerService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDetailDto> HandleAsync(CreateProjectCommand command, CancellationToken cancellationToken = default)
    {        var customerExists = await _customerService.CustomerExistsAsync(command.CustomerId, cancellationToken);
        if (!customerExists)
        {
            throw new BusinessRuleException($"Customer with ID '{command.CustomerId}' does not exist or is inactive/archived.");
        }

        var project = ProjectEntity.Create(
            command.CustomerId,
            command.Name,
            command.Type,
            command.CompanyId,
            command.ContactId,
            command.OpportunityId,
            command.ProposalId,
            command.Description,
            command.StartDate,
            command.PlannedDeliveryDate,
            command.Notes);

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDetailDto();
    }
}

public class UpdateProjectCommandHandler : ICommandHandler<UpdateProjectCommand, ProjectDetailDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDetailDto> HandleAsync(UpdateProjectCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.Id}' was not found.");
        }

        project.UpdateDetails(
            command.Name,
            command.Type,
            command.Description,
            command.StartDate,
            command.PlannedDeliveryDate,
            command.ActualDeliveryDate,
            command.CompanyId,
            command.ContactId,
            command.OpportunityId,
            command.ProposalId,
            command.Notes);

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDetailDto();
    }
}

public class StartProjectCommandHandler : ICommandHandler<StartProjectCommand, ProjectDetailDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StartProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDetailDto> HandleAsync(StartProjectCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.Id}' was not found.");
        }

        project.Start(command.StartDate);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDetailDto();
    }
}

public class ChangeProjectStatusCommandHandler : ICommandHandler<ChangeProjectStatusCommand, ProjectDetailDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeProjectStatusCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDetailDto> HandleAsync(ChangeProjectStatusCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.Id}' was not found.");
        }

        project.UpdateStatus(command.Status);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDetailDto();
    }
}

public class CompleteProjectCommandHandler : ICommandHandler<CompleteProjectCommand, ProjectDetailDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDetailDto> HandleAsync(CompleteProjectCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.Id}' was not found.");
        }

        project.Complete(command.ActualDeliveryDate);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDetailDto();
    }
}

public class CancelProjectCommandHandler : ICommandHandler<CancelProjectCommand, ProjectDetailDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectDetailDto> HandleAsync(CancelProjectCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.Id}' was not found.");
        }

        project.Cancel(command.Reason);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.ToDetailDto();
    }
}

public class ArchiveProjectCommandHandler : ICommandHandler<ArchiveProjectCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ArchiveProjectCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.Id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.Id}' was not found.");
        }

        project.Archive(command.ArchivedBy);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class AddRepositoryCommandHandler : ICommandHandler<AddRepositoryCommand, RepositoryDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddRepositoryCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RepositoryDto> HandleAsync(AddRepositoryCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.ProjectId}' was not found.");
        }

        var repository = project.AddRepository(command.Type, command.Url, command.Name, command.Branch, command.Description);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return repository.ToDto();
    }
}

public class UpdateRepositoryCommandHandler : ICommandHandler<UpdateRepositoryCommand, RepositoryDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRepositoryCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RepositoryDto> HandleAsync(UpdateRepositoryCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.ProjectId}' was not found.");
        }

        project.UpdateRepository(command.RepositoryId, command.Type, command.Url, command.Name, command.Branch, command.Description);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedRepo = project.Repositories.First(r => r.Id == command.RepositoryId);
        return updatedRepo.ToDto();
    }
}

public class RemoveRepositoryCommandHandler : ICommandHandler<RemoveRepositoryCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRepositoryCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(RemoveRepositoryCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.ProjectId}' was not found.");
        }

        project.RemoveRepository(command.RepositoryId);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class AddDeploymentCommandHandler : ICommandHandler<AddDeploymentCommand, DeploymentDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddDeploymentCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeploymentDto> HandleAsync(AddDeploymentCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.ProjectId}' was not found.");
        }

        var deployment = project.AddDeployment(
            command.Environment,
            command.Server,
            command.Provider,
            command.Domain,
            command.SslStatus,
            command.Version,
            command.Notes);

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return deployment.ToDto();
    }
}

public class UpdateDeploymentCommandHandler : ICommandHandler<UpdateDeploymentCommand, DeploymentDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDeploymentCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeploymentDto> HandleAsync(UpdateDeploymentCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.ProjectId}' was not found.");
        }

        project.UpdateDeployment(
            command.DeploymentId,
            command.Environment,
            command.Server,
            command.Provider,
            command.Domain,
            command.SslStatus,
            command.Version,
            command.Notes);

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedDeployment = project.Deployments.First(d => d.Id == command.DeploymentId);
        return updatedDeployment.ToDto();
    }
}

public class RemoveDeploymentCommandHandler : ICommandHandler<RemoveDeploymentCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveDeploymentCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(RemoveDeploymentCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.ProjectId}' was not found.");
        }

        project.RemoveDeployment(command.DeploymentId);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class SetRequirementCommandHandler : ICommandHandler<SetRequirementCommand, ProjectRequirementDto>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetRequirementCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProjectRequirementDto> HandleAsync(SetRequirementCommand command, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{command.ProjectId}' was not found.");
        }

        project.SetRequirement(command.BusinessGoal, command.Features, command.TechnologyNotes, command.TargetAudience);
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.Requirement!.ToDto();
    }
}
