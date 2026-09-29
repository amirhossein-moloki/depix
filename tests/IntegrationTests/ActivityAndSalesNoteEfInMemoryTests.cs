using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Infrastructure.Persistence.Repositories;
using Xunit;

namespace IntegrationTests;

public class ActivityAndSalesNoteEfInMemoryTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task AddAndGetActivity_ShouldPersistAndRetrieveActivity()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(AddAndGetActivity_ShouldPersistAndRetrieveActivity));
        await using var dbContext = new ApplicationDbContext(options);
        var activityRepository = new ActivityRepository(dbContext);

        var leadId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var activity = Activity.Create(
            leadId,
            userId,
            "CALL",
            "Discovery Call",
            "Discussed requirement details",
            "Successful",
            85,
            followUpAt: DateTime.UtcNow.AddDays(3),
            followUpNotes: "Send quote"
        );

        // Act
        await activityRepository.AddAsync(activity);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var readDbContext = new ApplicationDbContext(options);
        var readRepository = new ActivityRepository(readDbContext);
        var retrieved = await readRepository.GetByIdAsync(activity.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(leadId, retrieved.LeadId);
        Assert.Equal("CALL", retrieved.Type);
        Assert.Equal("Discovery Call", retrieved.Subject);
        Assert.Equal("Successful", retrieved.Result);
        Assert.Equal(85, retrieved.QualityScore);
        Assert.True(retrieved.IsFollowUpRequired);
    }

    [Fact]
    public async Task AddAndGetSalesNote_ShouldPersistAndRetrieveSalesNote()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(AddAndGetSalesNote_ShouldPersistAndRetrieveSalesNote));
        await using var dbContext = new ApplicationDbContext(options);
        var salesNoteRepository = new SalesNoteRepository(dbContext);

        var leadId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var note = SalesNote.Create(
            leadId,
            "Pre-sales Context",
            "Customer requires custom CMS",
            "Migration concerns",
            "Offer automated migration tool",
            80,
            createdBy
        );

        // Act
        await salesNoteRepository.AddAsync(note);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var readDbContext = new ApplicationDbContext(options);
        var readRepository = new SalesNoteRepository(readDbContext);
        var retrieved = await readRepository.GetByIdAsync(note.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(leadId, retrieved.LeadId);
        Assert.Equal("Pre-sales Context", retrieved.Title);
        Assert.Equal("Customer requires custom CMS", retrieved.NeedAnalysis);
        Assert.Equal(80, retrieved.Probability);
    }

    [Fact]
    public async Task SoftDeletedActivityAndSalesNote_ShouldBeFilteredByQueryFilter()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(SoftDeletedActivityAndSalesNote_ShouldBeFilteredByQueryFilter));
        await using var dbContext = new ApplicationDbContext(options);
        var activityRepo = new ActivityRepository(dbContext);
        var salesNoteRepo = new SalesNoteRepository(dbContext);

        var leadId = Guid.NewGuid();
        var activity = Activity.Create(leadId, Guid.NewGuid(), "CALL", "Call", "Desc", "Res", 60);
        var salesNote = SalesNote.Create(leadId, "Note", "Need", "Obj", "Strat", 50, Guid.NewGuid());

        await activityRepo.AddAsync(activity);
        await salesNoteRepo.AddAsync(salesNote);
        await dbContext.SaveChangesAsync();

        // Act
        activity.SoftDelete();
        salesNote.SoftDelete();
        activityRepo.Update(activity);
        salesNoteRepo.Update(salesNote);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var readDbContext = new ApplicationDbContext(options);
        var readActRepo = new ActivityRepository(readDbContext);
        var readNoteRepo = new SalesNoteRepository(readDbContext);

        var retrievedActivity = await readActRepo.GetByIdAsync(activity.Id);
        var retrievedNote = await readNoteRepo.GetByIdAsync(salesNote.Id);

        Assert.Null(retrievedActivity);
        Assert.Null(retrievedNote);
    }
}
