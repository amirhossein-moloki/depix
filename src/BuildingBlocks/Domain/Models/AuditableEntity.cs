namespace BuildingBlocks.Domain.Models;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    protected AuditableEntity(Guid id) : base(id) { }
    protected AuditableEntity() { }

    public void UpdateTimestamp(DateTime updatedAt, Guid? updatedBy = null)
    {
        UpdatedAt = updatedAt;
        UpdatedBy = updatedBy;
    }

    public void SetCreator(Guid? createdBy)
    {
        CreatedBy = createdBy;
    }
}

public abstract class AuditableAggregateRoot : AggregateRoot
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    protected AuditableAggregateRoot(Guid id) : base(id) { }
    protected AuditableAggregateRoot() { }

    public void UpdateTimestamp(DateTime updatedAt, Guid? updatedBy = null)
    {
        UpdatedAt = updatedAt;
        UpdatedBy = updatedBy;
    }

    public void SetCreator(Guid? createdBy)
    {
        CreatedBy = createdBy;
    }
}
