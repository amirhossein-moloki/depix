using Modules.Platform.Domain.Entities;
using Xunit;

namespace UnitTests;

public class PlatformDomainTests
{
    [Fact]
    public void CreateFileAsset_WithValidParameters_ShouldInitializeProperties()
    {
        // Arrange
        var entityType = "Lead";
        var entityId = Guid.NewGuid();
        var fileName = "contract.pdf";
        var originalFileName = "contract_v1.pdf";
        var contentType = "application/pdf";
        var extension = ".pdf";
        long size = 1024;
        var storageKey = "20261001/file123.pdf";
        var uploadedBy = Guid.NewGuid();

        // Act
        var file = FileAsset.Create(
            entityType,
            entityId,
            fileName,
            originalFileName,
            contentType,
            extension,
            size,
            storageKey,
            "LOCAL",
            "Initial contract",
            uploadedBy);

        // Assert
        Assert.NotEqual(Guid.Empty, file.Id);
        Assert.Equal(entityType, file.EntityType);
        Assert.Equal(entityId, file.EntityId);
        Assert.Equal(fileName, file.FileName);
        Assert.Equal(originalFileName, file.OriginalFileName);
        Assert.Equal(contentType, file.ContentType);
        Assert.Equal(extension, file.Extension);
        Assert.Equal(size, file.Size);
        Assert.Equal(storageKey, file.StorageKey);
        Assert.Equal("LOCAL", file.StorageProvider);
        Assert.Equal("Initial contract", file.Description);
        Assert.Equal(uploadedBy, file.UploadedBy);
        Assert.False(file.IsDeleted);
    }

    [Fact]
    public void SoftDeleteFileAsset_ShouldMarkIsDeleted()
    {
        // Arrange
        var file = FileAsset.Create("Company", Guid.NewGuid(), "logo.png", "logo.png", "image/png", ".png", 500, "key/logo.png");
        var deletedBy = Guid.NewGuid();

        // Act
        file.SoftDelete(deletedBy);

        // Assert
        Assert.True(file.IsDeleted);
        Assert.NotNull(file.DeletedAt);
        Assert.Equal(deletedBy, file.DeletedBy);
    }

    [Fact]
    public void CreateAuditLog_ShouldInitializeProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entityType = "Invoice";
        var entityId = Guid.NewGuid();

        // Act
        var log = AuditLog.Create(
            userId,
            entityType,
            entityId,
            "UPDATE",
            "OldStatus: DRAFT",
            "NewStatus: ISSUED",
            "corr-123",
            "127.0.0.1");

        // Assert
        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.Equal(userId, log.UserId);
        Assert.Equal(entityType, log.EntityType);
        Assert.Equal(entityId, log.EntityId);
        Assert.Equal("UPDATE", log.Action);
        Assert.Equal("OldStatus: DRAFT", log.OldValue);
        Assert.Equal("NewStatus: ISSUED", log.NewValue);
        Assert.Equal("corr-123", log.CorrelationId);
        Assert.Equal("127.0.0.1", log.IpAddress);
    }

    [Fact]
    public void CreateAppSetting_WithValidKey_ShouldInitializeProperties()
    {
        // Arrange & Act
        var setting = AppSetting.Create("Site.Title", "Depix Web App", "Website title", "General", true);

        // Assert
        Assert.NotEqual(Guid.Empty, setting.Id);
        Assert.Equal("Site.Title", setting.Key);
        Assert.Equal("Depix Web App", setting.Value);
        Assert.Equal("Website title", setting.Description);
        Assert.Equal("General", setting.Category);
        Assert.True(setting.IsActive);
        Assert.False(setting.IsDeleted);
    }

    [Fact]
    public void CreateAppSetting_WithEmptyKey_ShouldThrowException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => AppSetting.Create("", "value"));
    }

    [Fact]
    public void UpdateAppSetting_ShouldUpdateProperties()
    {
        // Arrange
        var setting = AppSetting.Create("Site.Theme", "Dark");

        // Act
        setting.Update("Light", "System color theme", "UI", false);

        // Assert
        Assert.Equal("Light", setting.Value);
        Assert.Equal("System color theme", setting.Description);
        Assert.Equal("UI", setting.Category);
        Assert.False(setting.IsActive);
        Assert.NotNull(setting.UpdatedAt);
    }

    [Fact]
    public void WorkTask_CreateAndUpdateStatus_ShouldWorkCorrectly()
    {
        // Arrange
        var assignedTo = Guid.NewGuid();

        // Act
        var task = WorkTask.Create(assignedTo, "Deploy release", "Deploy v1.2", "HIGH", "TODO");

        // Assert
        Assert.Equal("Deploy release", task.Title);
        Assert.Equal("TODO", task.Status);

        // Act
        task.UpdateStatus("IN_PROGRESS");

        // Assert
        Assert.Equal("IN_PROGRESS", task.Status);
    }
}
