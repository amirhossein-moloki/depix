using BuildingBlocks.Domain.Models;

namespace Modules.Platform.Domain.Entities;

public class FileAsset : Entity, ISoftDelete
{
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string Extension { get; private set; } = string.Empty;
    public long Size { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string StorageProvider { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? UploadedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private FileAsset() { }

    public FileAsset(
        Guid id,
        string entityType,
        Guid entityId,
        string fileName,
        string originalFileName,
        string contentType,
        string extension,
        long size,
        string storageKey,
        string storageProvider,
        string? description,
        Guid? uploadedBy,
        DateTime createdAt) : base(id)
    {
        EntityType = entityType;
        EntityId = entityId;
        FileName = fileName;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        Extension = extension;
        Size = size;
        StorageKey = storageKey;
        StorageProvider = storageProvider;
        Description = description;
        UploadedBy = uploadedBy;
        CreatedAt = createdAt;
    }

    public static FileAsset Create(
        string entityType,
        Guid entityId,
        string fileName,
        string originalFileName,
        string contentType,
        string extension,
        long size,
        string storageKey,
        string storageProvider = "LOCAL",
        string? description = null,
        Guid? uploadedBy = null)
    {
        return new FileAsset(
            Guid.NewGuid(),
            entityType,
            entityId,
            fileName,
            originalFileName,
            contentType,
            extension,
            size,
            storageKey,
            storageProvider,
            description,
            uploadedBy,
            DateTime.UtcNow);
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
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
