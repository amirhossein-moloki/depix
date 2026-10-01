using BuildingBlocks.Application.Contracts;
using Modules.Project.Domain.Repositories;

namespace Modules.Project.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
        return project != null;
    }
}
