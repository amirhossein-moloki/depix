using ProjectEntity = Modules.Project.Domain.Entities.Project;

namespace Modules.Project.Domain.Repositories;

public interface IProjectRepository
{
    Task<ProjectEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ProjectEntity project, CancellationToken cancellationToken = default);
    void Update(ProjectEntity project);
}
