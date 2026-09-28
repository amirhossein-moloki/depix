using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.ValueObjects;
using Modules.CRM.Infrastructure.Persistence.Repositories;
using Xunit;

namespace IntegrationTests;

public class CompanyEfInMemoryTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task AddAndGetCompany_ShouldPersistAndRetrieveCompany()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(AddAndGetCompany_ShouldPersistAndRetrieveCompany));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new CompanyRepository(dbContext);

        var company = Company.Create(
            "Acme Integration",
            "Software",
            "https://acme.org",
            "555-0199",
            "contact@acme.org",
            new Address("100 Enterprise Way"),
            "LEAD"
        );

        // Act
        await repository.AddAsync(company);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var readDbContext = new ApplicationDbContext(options);
        var readRepository = new CompanyRepository(readDbContext);
        var retrieved = await readRepository.GetByIdAsync(company.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Acme Integration", retrieved.Name);
        Assert.Equal("Software", retrieved.Industry);
        Assert.Equal("100 Enterprise Way", retrieved.Address.Text);
    }

    [Fact]
    public async Task SoftDeletedCompany_ShouldBeFilteredOutByDefaultQueryFilter()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(SoftDeletedCompany_ShouldBeFilteredOutByDefaultQueryFilter));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new CompanyRepository(dbContext);

        var company = Company.Create(
            "To Be Archived Ltd",
            "Logistics",
            "",
            "",
            "",
            new Address("404 Hidden Road"),
            "LEAD"
        );

        await repository.AddAsync(company);
        await dbContext.SaveChangesAsync();

        // Act: Soft Delete company
        company.SoftDelete(Guid.NewGuid());
        repository.Update(company);
        await dbContext.SaveChangesAsync();

        // Assert: standard DbContext query should exclude soft deleted entity
        await using var queryDbContext = new ApplicationDbContext(options);
        var fetched = await queryDbContext.Set<Company>().FirstOrDefaultAsync(c => c.Id == company.Id);
        Assert.Null(fetched);

        // Assert: IgnoreQueryFilters should still find soft deleted entity
        var softDeleted = await queryDbContext.Set<Company>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == company.Id);
        Assert.NotNull(softDeleted);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetListAsync_ShouldFilterAndPaginateCompanies()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(GetListAsync_ShouldFilterAndPaginateCompanies));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new CompanyRepository(dbContext);

        var c1 = Company.Create("Alpha Corp", "Tech", "", "", "alpha@test.com", new Address(""), "CUSTOMER");
        var c2 = Company.Create("Beta Inc", "Finance", "", "", "beta@test.com", new Address(""), "LEAD");
        var c3 = Company.Create("Alpha Services", "Tech", "", "", "services@alpha.com", new Address(""), "CUSTOMER");

        await repository.AddAsync(c1);
        await repository.AddAsync(c2);
        await repository.AddAsync(c3);
        await dbContext.SaveChangesAsync();

        // Act & Assert: Search by name
        var searchResult = await repository.GetListAsync(1, 10, search: "alpha", type: null, industry: null);
        Assert.Equal(2, searchResult.Count);

        // Act & Assert: Filter by type
        var typeResult = await repository.GetListAsync(1, 10, search: null, type: "CUSTOMER", industry: null);
        Assert.True(typeResult.All(c => c.Type == "CUSTOMER"));

        // Act & Assert: Pagination count
        var totalCount = await repository.CountAsync(search: null, type: null, industry: "Tech");
        Assert.True(totalCount >= 2);
    }
}
