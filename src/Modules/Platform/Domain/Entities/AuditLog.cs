using BuildingBlocks.Domain.Models;

namespace Modules.Platform.Domain.Entities;

public class AuditLog : Entity
{
    public Guid? UserId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string OldValue { get; private set; } = string.Empty;
    public string NewValue { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private AuditLog() { }

    public AuditLog(Guid id, Guid? userId, string entityType, Guid entityId, string action, string oldValue, string newValue, DateTime createdAt) : base(id)
    {
        UserId = userId;
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        OldValue = oldValue;
        NewValue = newValue;
        CreatedAt = createdAt;
    }

    public static AuditLog Create(Guid? userId, string entityType, Guid entityId, string action, string oldValue, string newValue)
    {
        return new AuditLog(Guid.NewGuid(), userId, entityType, entityId, action, oldValue, newValue, DateTime.UtcNow);
    }
}
