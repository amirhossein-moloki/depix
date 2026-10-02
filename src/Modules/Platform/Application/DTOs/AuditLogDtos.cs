using BuildingBlocks.Application.CQRS;

namespace Modules.Platform.Application.DTOs;

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string EntityType,
    Guid EntityId,
    string Action,
    string? OldValue,
    string? NewValue,
    string? CorrelationId,
    string? IpAddress,
    DateTime CreatedAt);

public record PagedAuditLogResult(
    List<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public record GetAuditLogsQuery(
    string? EntityType,
    Guid? EntityId,
    Guid? UserId,
    string? Action,
    int Page = 1,
    int PageSize = 10) : IQuery<PagedAuditLogResult>;

public record GetAuditLogByIdQuery(
    Guid Id) : IQuery<AuditLogDto>;
