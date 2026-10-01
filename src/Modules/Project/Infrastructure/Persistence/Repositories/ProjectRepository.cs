using Microsoft.EntityFrameworkCore;
using ProjectEntity = Modules.Project.Domain.Entities.Project;
using RepositoryEntity = Modules.Project.Domain.Entities.Repository;
using Modules.Project.Domain.Entities;
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

    public async Task<(List<ProjectEntity> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? status = null,
        Guid? customerId = null,
        Guid? companyId = null,
        Guid? opportunityId = null,
        Guid? proposalId = null,
        DateOnly? startDateFrom = null,
        DateOnly? startDateTo = null,
        DateOnly? plannedDeliveryDateFrom = null,
        DateOnly? plannedDeliveryDateTo = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<ProjectEntity>()
            .Include(p => p.Repositories)
            .Include(p => p.Deployments)
            .AsNoTracking();

        if (customerId.HasValue)
        {
            query = query.Where(p => p.CustomerId == customerId.Value);
        }

        if (companyId.HasValue)
        {
            query = query.Where(p => p.CompanyId == companyId.Value);
        }

        if (opportunityId.HasValue)
        {
            query = query.Where(p => p.OpportunityId == opportunityId.Value);
        }

        if (proposalId.HasValue)
        {
            query = query.Where(p => p.ProposalId == proposalId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim().ToUpperInvariant();
            query = query.Where(p => p.Status == normalizedStatus);
        }

        if (startDateFrom.HasValue)
        {
            query = query.Where(p => p.StartDate >= startDateFrom.Value);
        }

        if (startDateTo.HasValue)
        {
            query = query.Where(p => p.StartDate <= startDateTo.Value);
        }

        if (plannedDeliveryDateFrom.HasValue)
        {
            query = query.Where(p => p.PlannedDeliveryDate >= plannedDeliveryDateFrom.Value);
        }

        if (plannedDeliveryDateTo.HasValue)
        {
            query = query.Where(p => p.PlannedDeliveryDate <= plannedDeliveryDateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search.Trim().ToLower()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name.ToLower(), searchPattern) ||
                (p.Description != null && EF.Functions.Like(p.Description.ToLower(), searchPattern)) ||
                (p.Notes != null && EF.Functions.Like(p.Notes.ToLower(), searchPattern)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "status" => sortDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
            "startdate" => sortDescending ? query.OrderByDescending(p => p.StartDate) : query.OrderBy(p => p.StartDate),
            "planneddeliverydate" => sortDescending ? query.OrderByDescending(p => p.PlannedDeliveryDate) : query.OrderBy(p => p.PlannedDeliveryDate),
            _ => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ProjectEntity project, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<ProjectEntity>().AddAsync(project, cancellationToken);
    }

    public void Update(ProjectEntity project)
    {
        var entry = _dbContext.Entry(project);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Set<ProjectEntity>().Update(project);
        }

        foreach (var repo in project.Repositories)
        {
            if (_dbContext.Entry(repo).State == EntityState.Detached)
            {
                _dbContext.Set<RepositoryEntity>().Add(repo);
            }
        }

        foreach (var dep in project.Deployments)
        {
            if (_dbContext.Entry(dep).State == EntityState.Detached)
            {
                _dbContext.Set<Deployment>().Add(dep);
            }
        }

        if (project.Requirement != null && _dbContext.Entry(project.Requirement).State == EntityState.Detached)
        {
            _dbContext.Set<ProjectRequirement>().Add(project.Requirement);
        }
    }

    public void Delete(ProjectEntity project)
    {
        _dbContext.Set<ProjectEntity>().Remove(project);
    }
}
