using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Platform.Application.Commands;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Domain.Entities;
using Modules.Platform.Domain.Repositories;
using Modules.Platform.Domain.Services;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class PlatformCommandHandlerTests
{
    private readonly IFileAssetRepository _fileRepository = Substitute.For<IFileAssetRepository>();
    private readonly IFileStorage _fileStorage = Substitute.For<IFileStorage>();
    private readonly IAppSettingRepository _settingRepository = Substitute.For<IAppSettingRepository>();
    private readonly IWorkTaskRepository _taskRepository = Substitute.For<IWorkTaskRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task UploadFileHandler_ShouldSaveToStorageAndRepository()
    {
        // Arrange
        _fileStorage.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("storage/key/file1.pdf");

        var handler = new UploadFileCommandHandler(_fileRepository, _fileStorage, _unitOfWork);
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var command = new UploadFileCommand("Customer", Guid.NewGuid(), "doc.pdf", "doc_orig.pdf", "application/pdf", stream, 100, "Notes", Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Customer", result.EntityType);
        Assert.Equal("doc.pdf", result.FileName);
        Assert.Equal("storage/key/file1.pdf", result.StorageKey);
        await _fileRepository.Received(1).AddAsync(Arg.Is<FileAsset>(f => f.FileName == "doc.pdf"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteFileHandler_ShouldSoftDeleteAndUpdate()
    {
        // Arrange
        var file = FileAsset.Create("Project", Guid.NewGuid(), "spec.pdf", "spec.pdf", "application/pdf", ".pdf", 200, "storage/spec.pdf");
        _fileRepository.GetByIdAsync(file.Id, Arg.Any<CancellationToken>())
            .Returns(file);

        var handler = new DeleteFileCommandHandler(_fileRepository, _fileStorage, _unitOfWork);
        var command = new DeleteFileCommand(file.Id, Guid.NewGuid());

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(file.IsDeleted);
        _fileRepository.Received(1).Update(file);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _fileStorage.Received(1).DeleteAsync("storage/spec.pdf", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSettingHandler_ShouldAddAndSave_WhenKeyDoesNotExist()
    {
        // Arrange
        _settingRepository.ExistsKeyAsync("App.Env", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new CreateSettingCommandHandler(_settingRepository, _unitOfWork);
        var command = new CreateSettingCommand("App.Env", "Production", "Environment setting", "General", true);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("App.Env", result.Key);
        Assert.Equal("Production", result.Value);
        await _settingRepository.Received(1).AddAsync(Arg.Is<AppSetting>(s => s.Key == "App.Env"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSettingHandler_ShouldThrowBusinessRuleException_WhenKeyAlreadyExists()
    {
        // Arrange
        _settingRepository.ExistsKeyAsync("App.Env", null, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new CreateSettingCommandHandler(_settingRepository, _unitOfWork);
        var command = new CreateSettingCommand("App.Env", "Production", "", "General", true);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateSettingHandler_ShouldUpdateValue_WhenSettingExists()
    {
        // Arrange
        var setting = AppSetting.Create("Site.Mode", "Maintenance");
        _settingRepository.GetByKeyAsync("Site.Mode", Arg.Any<CancellationToken>())
            .Returns(setting);

        var handler = new UpdateSettingCommandHandler(_settingRepository, _unitOfWork);
        var command = new UpdateSettingCommand("Site.Mode", "Live", "Site operational mode", "System", true);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("Live", result.Value);
        Assert.Equal("System", result.Category);
        _settingRepository.Received(1).Update(setting);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTaskHandler_ShouldAddAndSave()
    {
        // Arrange
        var handler = new CreateTaskCommandHandler(_taskRepository, _unitOfWork);
        var command = new CreateTaskCommand(Guid.NewGuid(), "Fix Bug #101", "High severity issue", "URGENT", "TODO");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fix Bug #101", result.Title);
        Assert.Equal("URGENT", result.Priority);
        await _taskRepository.Received(1).AddAsync(Arg.Is<WorkTask>(t => t.Title == "Fix Bug #101"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateTaskStatusHandler_ShouldUpdateStatus()
    {
        // Arrange
        var task = WorkTask.Create(Guid.NewGuid(), "Task 1", "Desc", "MEDIUM", "TODO");
        _taskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(task);

        var handler = new UpdateTaskStatusCommandHandler(_taskRepository, _unitOfWork);
        var command = new UpdateTaskStatusCommand(task.Id, "DONE");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("DONE", result.Status);
        _taskRepository.Received(1).Update(task);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
