using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Domain.Repositories;

public interface ISalesVisitRepository
{
    Task<SalesVisit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SalesVisit>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<(List<SalesVisit> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? companyId = null,
        Guid? salespersonId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? resultFilter = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(SalesVisit visit, CancellationToken cancellationToken = default);
    void Update(SalesVisit visit);
}
