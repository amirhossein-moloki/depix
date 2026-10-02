using BuildingBlocks.Domain.Models;

namespace Modules.Platform.Domain.Entities;

public class AppSetting : AuditableAggregateRoot, ISoftDelete
{
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Category { get; private set; } = "General";
    public bool IsActive { get; private set; } = true;

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private AppSetting() { }

    public AppSetting(
        Guid id,
        string key,
        string value,
        string? description,
        string category,
        bool isActive) : base(id)
    {
        Key = key.Trim();
        Value = value;
        Description = description;
        Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();
        IsActive = isActive;
    }

    public static AppSetting Create(
        string key,
        string value,
        string? description = null,
        string category = "General",
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Setting key cannot be empty.", nameof(key));
        }

        return new AppSetting(Guid.NewGuid(), key, value, description, category, isActive);
    }

    public void Update(string value, string? description, string category, bool isActive)
    {
        Value = value;
        Description = description;
        Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim();
        IsActive = isActive;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
