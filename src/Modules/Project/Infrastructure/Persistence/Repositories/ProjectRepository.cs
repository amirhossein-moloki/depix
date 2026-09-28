using Microsoft.EntityFrameworkCore;
using ProjectEntity = Modules.Project.Domain.Entities.Project;
using Modules.Project.Domain.Repositories;

namespace Modules.Project.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly DbContext _dbContext;

    public ProjectRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProjectEntity>()
            .Include(p => p.Requirement)
            .Include(p => p.ProjectTechnologies)
                .ThenInclude(pt => pt.Technology)
            .Include(p => p.Repositories)
            .Include(p => p.Deployments)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(ProjectEntity project, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<ProjectEntity>().AddAsync(project, cancellationToken);
    }

    public void Update(ProjectEntity project)
    {
        _dbContext.Set<ProjectEntity>().Update(project);
    }
}
