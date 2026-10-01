using BuildingBlocks.Domain.ValueObjects;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Infrastructure.Persistence.Repositories;
using Xunit;

namespace IntegrationTests;

public class ProposalEfInMemoryTests
{
    static ProposalEfInMemoryTests()
    {
        _ = Modules.Identity.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.CRM.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Sales.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Customer.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Project.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Finance.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Support.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Platform.Infrastructure.AssemblyReference.Assembly;
    }

    private ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Repository_AddAndGetById_WithItems_ShouldPersistCorrectly()
    {
        using var dbContext = CreateDbContext();
        var proposalRepository = new ProposalRepository(dbContext);

        var opportunityId = Guid.NewGuid();
        var proposal = Proposal.Create(
            opportunityId,
            "Corporate Portal Proposal",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            version: "1.0",
            currency: "USD");

        proposal.AddItem("Frontend Design", "React UI development", 1, Money.Create(5000m, "USD"), Money.Create(500m, "USD"));
        proposal.AddItem("Backend API", ".NET Core micro-module API", 2, Money.Create(2500m, "USD"), Money.Create(0m, "USD"));

        await proposalRepository.AddAsync(proposal);
        await dbContext.SaveChangesAsync();

        var retrieved = await proposalRepository.GetByIdAsync(proposal.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Corporate Portal Proposal", retrieved.Title);
        Assert.Equal(opportunityId, retrieved.OpportunityId);
        Assert.Equal(2, retrieved.ProposalItems.Count);
        Assert.Equal(10000m, retrieved.Subtotal.Amount);
        Assert.Equal(500m, retrieved.Discount.Amount);
        Assert.Equal(9500m, retrieved.Total.Amount);
    }

    [Fact]
    public async Task Repository_GetByOpportunityId_ShouldReturnProposalsOrderedByCreatedDate()
    {
        using var dbContext = CreateDbContext();
        var proposalRepository = new ProposalRepository(dbContext);

        var opportunityId = Guid.NewGuid();
        var prop1 = Proposal.Create(opportunityId, "Proposal V1", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), "1.0");
        var prop2 = Proposal.Create(opportunityId, "Proposal V2", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), "2.0");

        await proposalRepository.AddAsync(prop1);
        await proposalRepository.AddAsync(prop2);
        await dbContext.SaveChangesAsync();

        var proposals = await proposalRepository.GetByOpportunityIdAsync(opportunityId);

        Assert.Equal(2, proposals.Count);
        Assert.All(proposals, p => Assert.Equal(opportunityId, p.OpportunityId));
    }

    [Fact]
    public async Task Repository_SoftDelete_ShouldNotReturnInQueries()
    {
        using var dbContext = CreateDbContext();
        var proposalRepository = new ProposalRepository(dbContext);

        var proposal = Proposal.Create(Guid.NewGuid(), "To Archive", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));
        await proposalRepository.AddAsync(proposal);
        await dbContext.SaveChangesAsync();

        proposal.SoftDelete();
        proposalRepository.Update(proposal);
        await dbContext.SaveChangesAsync();

        var retrieved = await proposalRepository.GetByIdAsync(proposal.Id);

        Assert.Null(retrieved);
    }
}
