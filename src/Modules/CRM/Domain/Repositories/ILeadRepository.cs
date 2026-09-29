using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Domain.Repositories;

public interface ILeadRepository
{
    Task<Lead?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Lead>> GetListAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        Guid? companyId,
        Guid? assignedTo,
        string? source,
        DateTime? fromDate,
        DateTime? toDate,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        string? search,
        string? status,
        Guid? companyId,
        Guid? assignedTo,
        string? source,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task AddAsync(Lead lead, CancellationToken cancellationToken = default);
    void Update(Lead lead);
}
