using BuildingBlocks.Domain.Models;

namespace Modules.Platform.Domain.Entities;

public class AuditLog : Entity
{
    public Guid? UserId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private AuditLog() { }

    public AuditLog(
        Guid id,
        Guid? userId,
        string entityType,
        Guid entityId,
        string action,
        string? oldValue,
        string? newValue,
        string? correlationId,
        string? ipAddress,
        DateTime createdAt) : base(id)
    {
        UserId = userId;
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        OldValue = oldValue;
        NewValue = newValue;
        CorrelationId = correlationId;
        IpAddress = ipAddress;
        CreatedAt = createdAt;
    }

    public static AuditLog Create(
        Guid? userId,
        string entityType,
        Guid entityId,
        string action,
        string? oldValue = null,
        string? newValue = null,
        string? correlationId = null,
        string? ipAddress = null)
    {
        return new AuditLog(
            Guid.NewGuid(),
            userId,
            entityType,
            entityId,
            action,
            oldValue,
            newValue,
            correlationId,
            ipAddress,
            DateTime.UtcNow);
    }
}
