using ProjectEntity = Modules.Project.Domain.Entities.Project;

namespace Modules.Project.Domain.Repositories;

public interface IProjectRepository
{
    Task<ProjectEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<ProjectEntity> Items, int TotalCount)> GetPagedAsync(
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
        CancellationToken cancellationToken = default);
    Task AddAsync(ProjectEntity project, CancellationToken cancellationToken = default);
    void Update(ProjectEntity project);
    void Delete(ProjectEntity project);
}
