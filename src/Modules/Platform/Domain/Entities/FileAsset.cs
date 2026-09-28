using BuildingBlocks.Domain.Models;

namespace Modules.Platform.Domain.Entities;

public class FileAsset : Entity
{
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string Path { get; private set; } = string.Empty;
    public Guid? UploadedBy { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private FileAsset() { }

    public FileAsset(Guid id, string entityType, Guid entityId, string fileName, string path, Guid? uploadedBy, DateTime createdAt) : base(id)
    {
        EntityType = entityType;
        EntityId = entityId;
        FileName = fileName;
        Path = path;
        UploadedBy = uploadedBy;
        CreatedAt = createdAt;
    }

    public static FileAsset Create(string entityType, Guid entityId, string fileName, string path, Guid? uploadedBy)
    {
        return new FileAsset(Guid.NewGuid(), entityType, entityId, fileName, path, uploadedBy, DateTime.UtcNow);
    }
}
