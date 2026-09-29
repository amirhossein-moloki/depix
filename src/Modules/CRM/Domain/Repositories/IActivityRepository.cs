using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Domain.Repositories;

public interface IActivityRepository
{
    Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Activity>> GetListAsync(
        int page,
        int pageSize,
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? userId,
        string? type,
        string? result,
        DateTime? fromDate,
        DateTime? toDate,
        string? search,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? userId,
        string? type,
        string? result,
        DateTime? fromDate,
        DateTime? toDate,
        string? search,
        CancellationToken cancellationToken = default);

    Task AddAsync(Activity activity, CancellationToken cancellationToken = default);
    void Update(Activity activity);
}
