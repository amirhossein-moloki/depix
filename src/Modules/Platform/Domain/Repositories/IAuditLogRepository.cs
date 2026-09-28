using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Domain.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
