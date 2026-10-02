using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        string? entityType,
        Guid? entityId,
        Guid? userId,
        string? action,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
