using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Project.Application.Commands;
using Modules.Project.Application.DTOs;
using Modules.Project.Application.Queries;
using ValidationException = BuildingBlocks.Common.Exceptions.ValidationException;

namespace Modules.Project.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ICommandHandler<CreateProjectCommand, ProjectDetailDto> _createProjectHandler;
    private readonly ICommandHandler<UpdateProjectCommand, ProjectDetailDto> _updateProjectHandler;
    private readonly ICommandHandler<StartProjectCommand, ProjectDetailDto> _startProjectHandler;
    private readonly ICommandHandler<ChangeProjectStatusCommand, ProjectDetailDto> _changeStatusHandler;
    private readonly ICommandHandler<CompleteProjectCommand, ProjectDetailDto> _completeProjectHandler;
    private readonly ICommandHandler<CancelProjectCommand, ProjectDetailDto> _cancelProjectHandler;
    private readonly ICommandHandler<ArchiveProjectCommand> _archiveProjectHandler;

    private readonly ICommandHandler<AddRepositoryCommand, RepositoryDto> _addRepositoryHandler;
    private readonly ICommandHandler<UpdateRepositoryCommand, RepositoryDto> _updateRepositoryHandler;
    private readonly ICommandHandler<RemoveRepositoryCommand> _removeRepositoryHandler;

    private readonly ICommandHandler<AddDeploymentCommand, DeploymentDto> _addDeploymentHandler;
    private readonly ICommandHandler<UpdateDeploymentCommand, DeploymentDto> _updateDeploymentHandler;
    private readonly ICommandHandler<RemoveDeploymentCommand> _removeDeploymentHandler;

    private readonly ICommandHandler<SetRequirementCommand, ProjectRequirementDto> _setRequirementHandler;

    private readonly IQueryHandler<GetProjectByIdQuery, ProjectDetailDto> _getProjectByIdHandler;
    private readonly IQueryHandler<GetProjectsQuery, PagedResult<ProjectListItemDto>> _getProjectsHandler;

    private readonly IValidator<CreateProjectCommand> _createValidator;
    private readonly IValidator<UpdateProjectCommand> _updateValidator;
    private readonly IValidator<ChangeProjectStatusCommand> _changeStatusValidator;
    private readonly IValidator<AddRepositoryCommand> _addRepositoryValidator;
    private readonly IValidator<UpdateRepositoryCommand> _updateRepositoryValidator;
    private readonly IValidator<AddDeploymentCommand> _addDeploymentValidator;
    private readonly IValidator<UpdateDeploymentCommand> _updateDeploymentValidator;

    public ProjectsController(
        ICommandHandler<CreateProjectCommand, ProjectDetailDto> createProjectHandler,
        ICommandHandler<UpdateProjectCommand, ProjectDetailDto> updateProjectHandler,
        ICommandHandler<StartProjectCommand, ProjectDetailDto> startProjectHandler,
        ICommandHandler<ChangeProjectStatusCommand, ProjectDetailDto> changeStatusHandler,
        ICommandHandler<CompleteProjectCommand, ProjectDetailDto> completeProjectHandler,
        ICommandHandler<CancelProjectCommand, ProjectDetailDto> cancelProjectHandler,
        ICommandHandler<ArchiveProjectCommand> archiveProjectHandler,
        ICommandHandler<AddRepositoryCommand, RepositoryDto> addRepositoryHandler,
        ICommandHandler<UpdateRepositoryCommand, RepositoryDto> updateRepositoryHandler,
        ICommandHandler<RemoveRepositoryCommand> removeRepositoryHandler,
        ICommandHandler<AddDeploymentCommand, DeploymentDto> addDeploymentHandler,
        ICommandHandler<UpdateDeploymentCommand, DeploymentDto> updateDeploymentHandler,
        ICommandHandler<RemoveDeploymentCommand> removeDeploymentHandler,
        ICommandHandler<SetRequirementCommand, ProjectRequirementDto> setRequirementHandler,
        IQueryHandler<GetProjectByIdQuery, ProjectDetailDto> getProjectByIdHandler,
        IQueryHandler<GetProjectsQuery, PagedResult<ProjectListItemDto>> getProjectsHandler,
        IValidator<CreateProjectCommand> createValidator,
        IValidator<UpdateProjectCommand> updateValidator,
        IValidator<ChangeProjectStatusCommand> changeStatusValidator,
        IValidator<AddRepositoryCommand> addRepositoryValidator,
        IValidator<UpdateRepositoryCommand> updateRepositoryValidator,
        IValidator<AddDeploymentCommand> addDeploymentValidator,
        IValidator<UpdateDeploymentCommand> updateDeploymentValidator)
    {
        _createProjectHandler = createProjectHandler;
        _updateProjectHandler = updateProjectHandler;
        _startProjectHandler = startProjectHandler;
        _changeStatusHandler = changeStatusHandler;
        _completeProjectHandler = completeProjectHandler;
        _cancelProjectHandler = cancelProjectHandler;
        _archiveProjectHandler = archiveProjectHandler;
        _addRepositoryHandler = addRepositoryHandler;
        _updateRepositoryHandler = updateRepositoryHandler;
        _removeRepositoryHandler = removeRepositoryHandler;
        _addDeploymentHandler = addDeploymentHandler;
        _updateDeploymentHandler = updateDeploymentHandler;
        _removeDeploymentHandler = removeDeploymentHandler;
        _setRequirementHandler = setRequirementHandler;
        _getProjectByIdHandler = getProjectByIdHandler;
        _getProjectsHandler = getProjectsHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _changeStatusValidator = changeStatusValidator;
        _addRepositoryValidator = addRepositoryValidator;
        _updateRepositoryValidator = updateRepositoryValidator;
        _addDeploymentValidator = addDeploymentValidator;
        _updateDeploymentValidator = updateDeploymentValidator;
    }

    [HttpPost]
    [Authorize(Policy = "Project.Create")]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand(
            request.CustomerId,
            request.Name,
            request.Type,
            request.CompanyId,
            request.ContactId,
            request.OpportunityId,
            request.ProposalId,
            request.Description,
            request.StartDate,
            request.PlannedDeliveryDate,
            request.Notes
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Project creation validation failed.", errors);
        }

        var project = await _createProjectHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, ResponseFactory.Success(project, "Project created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "Project.Read")]
    public async Task<IActionResult> GetProjects(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? opportunityId = null,
        [FromQuery] Guid? proposalId = null,
        [FromQuery] DateOnly? startDateFrom = null,
        [FromQuery] DateOnly? startDateTo = null,
        [FromQuery] DateOnly? plannedDeliveryDateFrom = null,
        [FromQuery] DateOnly? plannedDeliveryDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProjectsQuery(
            page,
            pageSize,
            search,
            status,
            customerId,
            companyId,
            opportunityId,
            proposalId,
            startDateFrom,
            startDateTo,
            plannedDeliveryDateFrom,
            plannedDeliveryDateTo,
            sortBy,
            sortDescending);

        var result = await _getProjectsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Projects retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Project.Read")]
    public async Task<IActionResult> GetProjectById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProjectByIdQuery(id);
        var project = await _getProjectByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(project, "Project details retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Project.Update")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand(
            id,
            request.Name,
            request.Type,
            request.Description,
            request.StartDate,
            request.PlannedDeliveryDate,
            request.ActualDeliveryDate,
            request.CompanyId,
            request.ContactId,
            request.OpportunityId,
            request.ProposalId,
            request.Notes
        );

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Project update validation failed.", errors);
        }

        var project = await _updateProjectHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(project, "Project updated successfully."));
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Policy = "Project.StatusChange")]
    public async Task<IActionResult> StartProject(Guid id, [FromBody] StartProjectRequest? request, CancellationToken cancellationToken)
    {
        var command = new StartProjectCommand(id, request?.StartDate);
        var project = await _startProjectHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(project, "Project started successfully."));
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = "Project.StatusChange")]
    public async Task<IActionResult> ChangeProjectStatus(Guid id, [FromBody] ChangeProjectStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeProjectStatusCommand(id, request.Status);

        var validationResult = await _changeStatusValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Project status change validation failed.", errors);
        }

        var project = await _changeStatusHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(project, "Project status updated successfully."));
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "Project.StatusChange")]
    public async Task<IActionResult> CompleteProject(Guid id, [FromBody] CompleteProjectRequest? request, CancellationToken cancellationToken)
    {
        var command = new CompleteProjectCommand(id, request?.ActualDeliveryDate);
        var project = await _completeProjectHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(project, "Project completed successfully."));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "Project.StatusChange")]
    public async Task<IActionResult> CancelProject(Guid id, [FromBody] CancelProjectRequest? request, CancellationToken cancellationToken)
    {
        var command = new CancelProjectCommand(id, request?.Reason);
        var project = await _cancelProjectHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(project, "Project cancelled successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Project.Delete")]
    public async Task<IActionResult> ArchiveProject(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? archivedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new ArchiveProjectCommand(id, archivedBy);
        await _archiveProjectHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Project archived successfully.", string.Empty));
    }

    [HttpPost("{id:guid}/repositories")]
    [Authorize(Policy = "Project.Repository.Manage")]
    public async Task<IActionResult> AddRepository(Guid id, [FromBody] AddRepositoryRequest request, CancellationToken cancellationToken)
    {
        var command = new AddRepositoryCommand(id, request.Type, request.Url, request.Name, request.Branch, request.Description);

        var validationResult = await _addRepositoryValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Add repository validation failed.", errors);
        }

        var repository = await _addRepositoryHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(repository, "Repository added successfully."));
    }

    [HttpPut("{id:guid}/repositories/{repositoryId:guid}")]
    [Authorize(Policy = "Project.Repository.Manage")]
    public async Task<IActionResult> UpdateRepository(Guid id, Guid repositoryId, [FromBody] UpdateRepositoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateRepositoryCommand(id, repositoryId, request.Type, request.Url, request.Name, request.Branch, request.Description);

        var validationResult = await _updateRepositoryValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Update repository validation failed.", errors);
        }

        var repository = await _updateRepositoryHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(repository, "Repository updated successfully."));
    }

    [HttpDelete("{id:guid}/repositories/{repositoryId:guid}")]
    [Authorize(Policy = "Project.Repository.Manage")]
    public async Task<IActionResult> RemoveRepository(Guid id, Guid repositoryId, CancellationToken cancellationToken)
    {
        var command = new RemoveRepositoryCommand(id, repositoryId);
        await _removeRepositoryHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Repository removed successfully.", string.Empty));
    }

    [HttpPost("{id:guid}/deployments")]
    [Authorize(Policy = "Project.Deployment.Manage")]
    public async Task<IActionResult> AddDeployment(Guid id, [FromBody] AddDeploymentRequest request, CancellationToken cancellationToken)
    {
        var command = new AddDeploymentCommand(
            id,
            request.Environment,
            request.Server,
            request.Provider,
            request.Domain,
            request.SslStatus,
            request.Version,
            request.Notes);

        var validationResult = await _addDeploymentValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Add deployment validation failed.", errors);
        }

        var deployment = await _addDeploymentHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(deployment, "Deployment added successfully."));
    }

    [HttpPut("{id:guid}/deployments/{deploymentId:guid}")]
    [Authorize(Policy = "Project.Deployment.Manage")]
    public async Task<IActionResult> UpdateDeployment(Guid id, Guid deploymentId, [FromBody] UpdateDeploymentRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDeploymentCommand(
            id,
            deploymentId,
            request.Environment,
            request.Server,
            request.Provider,
            request.Domain,
            request.SslStatus,
            request.Version,
            request.Notes);

        var validationResult = await _updateDeploymentValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new ValidationException("Update deployment validation failed.", errors);
        }

        var deployment = await _updateDeploymentHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(deployment, "Deployment updated successfully."));
    }

    [HttpDelete("{id:guid}/deployments/{deploymentId:guid}")]
    [Authorize(Policy = "Project.Deployment.Manage")]
    public async Task<IActionResult> RemoveDeployment(Guid id, Guid deploymentId, CancellationToken cancellationToken)
    {
        var command = new RemoveDeploymentCommand(id, deploymentId);
        await _removeDeploymentHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Deployment removed successfully.", string.Empty));
    }

    [HttpPost("{id:guid}/requirement")]
    [Authorize(Policy = "Project.Update")]
    public async Task<IActionResult> SetRequirement(Guid id, [FromBody] SetRequirementRequest request, CancellationToken cancellationToken)
    {
        var command = new SetRequirementCommand(id, request.BusinessGoal, request.Features, request.TechnologyNotes, request.TargetAudience);
        var requirement = await _setRequirementHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(requirement, "Project requirement set successfully."));
    }
}
