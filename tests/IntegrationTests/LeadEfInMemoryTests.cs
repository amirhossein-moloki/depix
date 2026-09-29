using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Infrastructure.Persistence.Repositories;
using Xunit;

namespace IntegrationTests;

public class LeadEfInMemoryTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task AddAndGetLead_ShouldPersistAndRetrieveLead()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(AddAndGetLead_ShouldPersistAndRetrieveLead));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new LeadRepository(dbContext);

        var companyId = Guid.NewGuid();
        var lead = Lead.Create(
            companyId,
            "Custom Website Development",
            "Inbound Form",
            "e-commerce portal with payment gateway",
            45000m
        );

        // Act
        await repository.AddAsync(lead);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var readDbContext = new ApplicationDbContext(options);
        var readRepository = new LeadRepository(readDbContext);
        var retrieved = await readRepository.GetByIdAsync(lead.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Custom Website Development", retrieved.Title);
        Assert.Equal("Inbound Form", retrieved.Source);
        Assert.Equal(45000m, retrieved.EstimatedValue);
        Assert.Equal("NEW", retrieved.Status);
    }

    [Fact]
    public async Task SoftDeletedLead_ShouldBeFilteredOutByDefaultQueryFilter()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(SoftDeletedLead_ShouldBeFilteredOutByDefaultQueryFilter));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new LeadRepository(dbContext);

        var lead = Lead.Create(Guid.NewGuid(), "To Be Archived Lead", "Web");
        await repository.AddAsync(lead);
        await dbContext.SaveChangesAsync();

        // Act: Soft Delete lead
        lead.SoftDelete(Guid.NewGuid());
        repository.Update(lead);
        await dbContext.SaveChangesAsync();

        // Assert: Standard DbContext query should exclude soft deleted entity
        await using var queryDbContext = new ApplicationDbContext(options);
        var fetched = await queryDbContext.Set<Lead>().FirstOrDefaultAsync(l => l.Id == lead.Id);
        Assert.Null(fetched);

        // Assert: IgnoreQueryFilters should still find soft deleted entity
        var softDeleted = await queryDbContext.Set<Lead>().IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == lead.Id);
        Assert.NotNull(softDeleted);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetListAsync_ShouldFilterAndPaginateLeads()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(GetListAsync_ShouldFilterAndPaginateLeads));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new LeadRepository(dbContext);

        var companyId = Guid.NewGuid();
        var l1 = Lead.Create(companyId, "Alpha Portal", "Web", estimatedValue: 10000m);
        var l2 = Lead.Create(companyId, "Beta Mobile App", "Referral", estimatedValue: 20000m);
        l2.Qualify(); // QUALIFIED
        var l3 = Lead.Create(companyId, "Alpha Redesign", "Web", estimatedValue: 15000m);

        await repository.AddAsync(l1);
        await repository.AddAsync(l2);
        await repository.AddAsync(l3);
        await dbContext.SaveChangesAsync();

        // Act & Assert: Search by title
        var searchResult = await repository.GetListAsync(
            1, 10, search: "Alpha", status: null, companyId: null, assignedTo: null, source: null,
            fromDate: null, toDate: null, sortBy: null, sortDescending: true);
        Assert.Equal(2, searchResult.Count);

        // Act & Assert: Filter by status
        var statusResult = await repository.GetListAsync(
            1, 10, search: null, status: "QUALIFIED", companyId: null, assignedTo: null, source: null,
            fromDate: null, toDate: null, sortBy: null, sortDescending: true);
        Assert.Single(statusResult);
        Assert.Equal("Beta Mobile App", statusResult[0].Title);

        // Act & Assert: CountAsync
        var count = await repository.CountAsync(
            search: null, status: null, companyId: companyId, assignedTo: null, source: null,
            fromDate: null, toDate: null);
        Assert.Equal(3, count);
    }
}
