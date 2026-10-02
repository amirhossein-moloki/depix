using BuildingBlocks.Common.Exceptions;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Application.Queries;
using Modules.Platform.Domain.Entities;
using Modules.Platform.Domain.Repositories;
using Modules.Platform.Domain.Services;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class PlatformQueryHandlerTests
{
    private readonly IFileAssetRepository _fileRepository = Substitute.For<IFileAssetRepository>();
    private readonly IFileStorage _fileStorage = Substitute.For<IFileStorage>();
    private readonly IAuditLogRepository _auditRepository = Substitute.For<IAuditLogRepository>();
    private readonly IAppSettingRepository _settingRepository = Substitute.For<IAppSettingRepository>();
    private readonly IWorkTaskRepository _taskRepository = Substitute.For<IWorkTaskRepository>();

    [Fact]
    public async Task GetFileMetadataQuery_ShouldReturnDto_WhenFileExists()
    {
        // Arrange
        var file = FileAsset.Create("Lead", Guid.NewGuid(), "file.pdf", "file.pdf", "application/pdf", ".pdf", 500, "key/file.pdf");
        _fileRepository.GetByIdAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns(file);

        var handler = new GetFileMetadataQueryHandler(_fileRepository);

        // Act
        var result = await handler.HandleAsync(new GetFileMetadataQuery(file.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(file.Id, result.Id);
        Assert.Equal("file.pdf", result.FileName);
    }

    [Fact]
    public async Task GetFileMetadataQuery_ShouldThrowNotFound_WhenFileDoesNotExist()
    {
        // Arrange
        _fileRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((FileAsset?)null);

        var handler = new GetFileMetadataQueryHandler(_fileRepository);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(new GetFileMetadataQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task GetAuditLogsQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            AuditLog.Create(Guid.NewGuid(), "Lead", Guid.NewGuid(), "CREATE"),
            AuditLog.Create(Guid.NewGuid(), "Lead", Guid.NewGuid(), "UPDATE")
        };

        _auditRepository.GetPagedAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), 1, 10, Arg.Any<CancellationToken>())
            .Returns((logs, 2));

        var handler = new GetAuditLogsQueryHandler(_auditRepository);

        // Act
        var result = await handler.HandleAsync(new GetAuditLogsQuery("Lead", null, null, null, 1, 10), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetSettingByKeyQuery_ShouldReturnDto_WhenSettingExists()
    {
        // Arrange
        var setting = AppSetting.Create("Feature.X", "Enabled");
        _settingRepository.GetByKeyAsync("Feature.X", Arg.Any<CancellationToken>())
            .Returns(setting);

        var handler = new GetSettingByKeyQueryHandler(_settingRepository);

        // Act
        var result = await handler.HandleAsync(new GetSettingByKeyQuery("Feature.X"), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Feature.X", result.Key);
        Assert.Equal("Enabled", result.Value);
    }

    [Fact]
    public async Task GetTasksQuery_ShouldReturnList()
    {
        // Arrange
        var tasks = new List<WorkTask>
        {
            WorkTask.Create(Guid.NewGuid(), "Task A", "Desc A"),
            WorkTask.Create(Guid.NewGuid(), "Task B", "Desc B")
        };

        _taskRepository.GetListAsync(Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(tasks);

        var handler = new GetTasksQueryHandler(_taskRepository);

        // Act
        var result = await handler.HandleAsync(new GetTasksQuery(null, null), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
    }
}
