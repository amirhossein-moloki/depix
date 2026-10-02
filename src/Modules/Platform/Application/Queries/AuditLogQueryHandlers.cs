using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Application.Mappings;
using Modules.Platform.Domain.Repositories;

namespace Modules.Platform.Application.Queries;

public class GetAuditLogsQueryHandler : IQueryHandler<GetAuditLogsQuery, PagedAuditLogResult>
{
    private readonly IAuditLogRepository _repository;

    public GetAuditLogsQueryHandler(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedAuditLogResult> HandleAsync(GetAuditLogsQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(
            query.EntityType,
            query.EntityId,
            query.UserId,
            query.Action,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(a => a.ToDto()).ToList();
        return new PagedAuditLogResult(dtos, totalCount, page, pageSize);
    }
}

public class GetAuditLogByIdQueryHandler : IQueryHandler<GetAuditLogByIdQuery, AuditLogDto>
{
    private readonly IAuditLogRepository _repository;

    public GetAuditLogByIdQueryHandler(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditLogDto> HandleAsync(GetAuditLogByIdQuery query, CancellationToken cancellationToken = default)
    {
        var auditLog = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (auditLog == null)
        {
            throw new EntityNotFoundException("AuditLog", query.Id);
        }

        return auditLog.ToDto();
    }
}
