using Modules.Platform.Application.DTOs;
using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Application.Mappings;

public static class PlatformMappingExtensions
{
    public static FileAssetDto ToDto(this FileAsset file)
    {
        return new FileAssetDto(
            file.Id,
            file.EntityType,
            file.EntityId,
            file.FileName,
            file.OriginalFileName,
            file.ContentType,
            file.Extension,
            file.Size,
            file.StorageKey,
            file.StorageProvider,
            file.Description,
            file.UploadedBy,
            file.CreatedAt);
    }

    public static AuditLogDto ToDto(this AuditLog log)
    {
        return new AuditLogDto(
            log.Id,
            log.UserId,
            log.EntityType,
            log.EntityId,
            log.Action,
            log.OldValue,
            log.NewValue,
            log.CorrelationId,
            log.IpAddress,
            log.CreatedAt);
    }

    public static AppSettingDto ToDto(this AppSetting setting)
    {
        return new AppSettingDto(
            setting.Id,
            setting.Key,
            setting.Value,
            setting.Description,
            setting.Category,
            setting.IsActive,
            setting.CreatedAt,
            setting.UpdatedAt);
    }

    public static WorkTaskDto ToDto(this WorkTask task)
    {
        return new WorkTaskDto(
            task.Id,
            task.AssignedTo,
            task.Title,
            task.Description,
            task.Priority,
            task.Status,
            task.Deadline,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
