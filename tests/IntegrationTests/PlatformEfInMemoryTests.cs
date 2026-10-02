using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.Platform.Domain.Entities;
using Modules.Platform.Infrastructure.Persistence.Repositories;
using Xunit;

namespace IntegrationTests;

public class PlatformEfInMemoryTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task AddAndGetFileAsset_ShouldPersistAndRetrieveFileAsset()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(AddAndGetFileAsset_ShouldPersistAndRetrieveFileAsset));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new FileAssetRepository(dbContext);

        var entityId = Guid.NewGuid();
        var file = FileAsset.Create(
            "Customer",
            entityId,
            "invoice_101.pdf",
            "invoice_101_orig.pdf",
            "application/pdf",
            ".pdf",
            2048,
            "20261001/inv101.pdf",
            "LOCAL",
            "Customer monthly invoice",
            Guid.NewGuid()
        );

        // Act
        await repository.AddAsync(file);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var readDbContext = new ApplicationDbContext(options);
        var readRepository = new FileAssetRepository(readDbContext);
        var retrieved = await readRepository.GetByIdAsync(file.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Customer", retrieved.EntityType);
        Assert.Equal(entityId, retrieved.EntityId);
        Assert.Equal("invoice_101.pdf", retrieved.FileName);
        Assert.Equal(2048, retrieved.Size);

        var entityFiles = await readRepository.GetByEntityAsync("Customer", entityId);
        Assert.Single(entityFiles);
    }

    [Fact]
    public async Task SoftDeletedFileAsset_ShouldBeFilteredOutByDefaultQueryFilter()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(SoftDeletedFileAsset_ShouldBeFilteredOutByDefaultQueryFilter));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new FileAssetRepository(dbContext);

        var file = FileAsset.Create("SupportTicket", Guid.NewGuid(), "screenshot.png", "screenshot.png", "image/png", ".png", 1024, "key/screenshot.png");
        await repository.AddAsync(file);
        await dbContext.SaveChangesAsync();

        // Act
        file.SoftDelete(Guid.NewGuid());
        repository.Update(file);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var queryDbContext = new ApplicationDbContext(options);
        var fetched = await queryDbContext.Set<FileAsset>().FirstOrDefaultAsync(f => f.Id == file.Id);
        Assert.Null(fetched);

        var softDeleted = await queryDbContext.Set<FileAsset>().IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == file.Id);
        Assert.NotNull(softDeleted);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task AddAndQueryAuditLogs_ShouldSupportPagedQueries()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(AddAndQueryAuditLogs_ShouldSupportPagedQueries));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new AuditLogRepository(dbContext);

        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var log1 = AuditLog.Create(userId, "Company", entityId, "CREATE", null, "Name: Acme");
        var log2 = AuditLog.Create(userId, "Company", entityId, "UPDATE", "Name: Acme", "Name: Acme Corp");
        var log3 = AuditLog.Create(Guid.NewGuid(), "Lead", Guid.NewGuid(), "CREATE", null, "Name: Lead A");

        await repository.AddAsync(log1);
        await repository.AddAsync(log2);
        await repository.AddAsync(log3);
        await dbContext.SaveChangesAsync();

        // Act
        await using var queryDbContext = new ApplicationDbContext(options);
        var queryRepo = new AuditLogRepository(queryDbContext);
        var (items, totalCount) = await queryRepo.GetPagedAsync("Company", entityId, userId, null, 1, 10);

        // Assert
        Assert.Equal(2, totalCount);
        Assert.Equal(2, items.Count);
        Assert.All(items, log => Assert.Equal("Company", log.EntityType));
    }

    [Fact]
    public async Task AppSetting_AddAndGetByKey_ShouldPersistAndFilterDeleted()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(AppSetting_AddAndGetByKey_ShouldPersistAndFilterDeleted));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new AppSettingRepository(dbContext);

        var setting1 = AppSetting.Create("System.MaintenanceMode", "false", "Toggles maintenance mode", "System", true);
        var setting2 = AppSetting.Create("Mail.SmtpHost", "smtp.office365.com", "SMTP Host", "Email", true);

        await repository.AddAsync(setting1);
        await repository.AddAsync(setting2);
        await dbContext.SaveChangesAsync();

        // Act & Assert
        await using var queryDbContext = new ApplicationDbContext(options);
        var queryRepo = new AppSettingRepository(queryDbContext);

        var retrieved = await queryRepo.GetByKeyAsync("System.MaintenanceMode");
        Assert.NotNull(retrieved);
        Assert.Equal("false", retrieved.Value);

        var exists = await queryRepo.ExistsKeyAsync("Mail.SmtpHost");
        Assert.True(exists);

        // Act: Soft Delete
        retrieved.SoftDelete();
        queryRepo.Update(retrieved);
        await queryDbContext.SaveChangesAsync();

        // Assert: should be null under default query filter
        await using var finalDbContext = new ApplicationDbContext(options);
        var finalRepo = new AppSettingRepository(finalDbContext);
        var deletedSetting = await finalRepo.GetByKeyAsync("System.MaintenanceMode");
        Assert.Null(deletedSetting);
    }

    [Fact]
    public async Task WorkTask_PersistenceAndQuerying_ShouldWorkCorrectly()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(WorkTask_PersistenceAndQuerying_ShouldWorkCorrectly));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new WorkTaskRepository(dbContext);

        var user = Guid.NewGuid();
        var task1 = WorkTask.Create(user, "Task 1", "Desc 1", "HIGH", "TODO");
        var task2 = WorkTask.Create(user, "Task 2", "Desc 2", "LOW", "DONE");

        await repository.AddAsync(task1);
        await repository.AddAsync(task2);
        await dbContext.SaveChangesAsync();

        // Act
        await using var queryDbContext = new ApplicationDbContext(options);
        var queryRepo = new WorkTaskRepository(queryDbContext);

        var userTasks = await queryRepo.GetListAsync(user, "TODO");
        Assert.Single(userTasks);
        Assert.Equal("Task 1", userTasks[0].Title);
    }
}
