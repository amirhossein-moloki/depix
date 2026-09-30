using BuildingBlocks.Domain.ValueObjects;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Repositories;
using Modules.Sales.Infrastructure.Persistence.Repositories;
using Xunit;

namespace IntegrationTests;

public class OpportunityEfInMemoryTests
{
    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Repository_AddAndGetById_ShouldPersistCorrectly()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repository = new OpportunityRepository(dbContext);

        var opportunity = Opportunity.Create(
            title: "E-Commerce System",
            description: "Custom storefront development",
            companyId: Guid.NewGuid(),
            customerId: Guid.NewGuid(),
            stage: OpportunityStage.Discovery,
            value: Money.Create(30000, "USD")
        );

        // Act
        await repository.AddAsync(opportunity);
        await dbContext.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(opportunity.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("E-Commerce System", retrieved.Title);
        Assert.Equal(30000, retrieved.Value.Amount);
        Assert.Equal("USD", retrieved.Value.Currency);
        Assert.Equal(OpportunityStage.Discovery, retrieved.Stage);
        Assert.Equal(OpportunityStatus.Open, retrieved.Status);
    }

    [Fact]
    public async Task Repository_GetFiltered_ShouldApplyFiltersAndPagination()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repository = new OpportunityRepository(dbContext);

        var companyId = Guid.NewGuid();
        var assignedTo = Guid.NewGuid();

        var opp1 = Opportunity.Create("Alpha Portal", companyId: companyId, assignedTo: assignedTo, stage: OpportunityStage.New, value: Money.Create(5000, "USD"));
        var opp2 = Opportunity.Create("Beta Redesign", companyId: companyId, assignedTo: assignedTo, stage: OpportunityStage.Qualified, value: Money.Create(15000, "USD"));
        var opp3 = Opportunity.Create("Gamma Site", companyId: Guid.NewGuid(), stage: OpportunityStage.Proposal, value: Money.Create(25000, "USD"));

        await repository.AddAsync(opp1);
        await repository.AddAsync(opp2);
        await repository.AddAsync(opp3);
        await dbContext.SaveChangesAsync();

        // Act
        var filterParams = new OpportunityFilterParams(
            Page: 1,
            PageSize: 10,
            CompanyId: companyId,
            AssignedTo: assignedTo
        );

        var (items, totalCount) = await repository.GetFilteredAsync(filterParams);

        // Assert
        Assert.Equal(2, totalCount);
        Assert.All(items, o => Assert.Equal(companyId, o.CompanyId));
        Assert.All(items, o => Assert.Equal(assignedTo, o.AssignedTo));
    }

    [Fact]
    public async Task Repository_GetPipelineSummary_ShouldGroupAllStages()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repository = new OpportunityRepository(dbContext);

        var opp1 = Opportunity.Create("Opp 1", stage: OpportunityStage.New, value: Money.Create(10000, "USD"));
        var opp2 = Opportunity.Create("Opp 2", stage: OpportunityStage.New, value: Money.Create(15000, "USD"));
        var opp3 = Opportunity.Create("Opp 3", stage: OpportunityStage.Won, value: Money.Create(50000, "USD"));

        await repository.AddAsync(opp1);
        await repository.AddAsync(opp2);
        await repository.AddAsync(opp3);
        await dbContext.SaveChangesAsync();

        // Act
        var pipeline = await repository.GetPipelineSummaryAsync();

        // Assert
        Assert.NotNull(pipeline);
        Assert.Equal(OpportunityStage.AllStages.Count, pipeline.Count);

        var newSummary = pipeline.First(s => s.Stage == OpportunityStage.New);
        Assert.Equal(2, newSummary.Count);
        Assert.Equal(25000, newSummary.TotalValue);

        var wonSummary = pipeline.First(s => s.Stage == OpportunityStage.Won);
        Assert.Equal(1, wonSummary.Count);
        Assert.Equal(50000, wonSummary.TotalValue);
    }

    [Fact]
    public async Task Repository_SoftDeleted_ShouldNotBeReturnedInQueries()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repository = new OpportunityRepository(dbContext);

        var activeOpp = Opportunity.Create("Active Opportunity");
        var deletedOpp = Opportunity.Create("Deleted Opportunity");

        await repository.AddAsync(activeOpp);
        await repository.AddAsync(deletedOpp);
        await dbContext.SaveChangesAsync();

        // Act - Soft delete
        deletedOpp.SoftDelete();
        repository.Update(deletedOpp);
        await dbContext.SaveChangesAsync();

        var retrievedActive = await repository.GetByIdAsync(activeOpp.Id);
        var retrievedDeleted = await repository.GetByIdAsync(deletedOpp.Id);
        var (filteredItems, totalCount) = await repository.GetFilteredAsync(new OpportunityFilterParams());

        // Assert
        Assert.NotNull(retrievedActive);
        Assert.Null(retrievedDeleted);
        Assert.Equal(1, totalCount);
        Assert.Single(filteredItems);
        Assert.Equal(activeOpp.Id, filteredItems[0].Id);
    }
}
