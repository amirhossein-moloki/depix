using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Domain.Repositories;

public interface ISalesVisitRepository
{
    Task<SalesVisit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SalesVisit>> GetByCompanyIdAsync(Guid companyId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<List<SalesVisit>> GetVisitsAsync(
        Guid? companyId = null,
        Guid? salespersonId = null,
        string? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    );
    Task<int> GetVisitsCountAsync(
        Guid? companyId = null,
        Guid? salespersonId = null,
        string? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(SalesVisit visit, CancellationToken cancellationToken = default);
    Task UpdateAsync(SalesVisit visit, CancellationToken cancellationToken = default);
}
