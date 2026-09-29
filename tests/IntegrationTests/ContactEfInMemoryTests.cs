using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Infrastructure.Persistence.Repositories;
using Xunit;

namespace IntegrationTests;

public class ContactEfInMemoryTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task AddAndGetContact_ShouldPersistAndRetrieveContact()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(AddAndGetContact_ShouldPersistAndRetrieveContact));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new ContactRepository(dbContext);

        var companyId = Guid.NewGuid();
        var contact = Contact.Create(
            companyId,
            "John",
            "Smith",
            "john.smith@integration.org",
            "555-0100",
            "Director",
            "Important Contact",
            isDecisionMaker: true,
            influenceLevel: "High"
        );

        // Act
        await repository.AddAsync(contact);
        await dbContext.SaveChangesAsync();

        // Assert
        await using var readDbContext = new ApplicationDbContext(options);
        var readRepository = new ContactRepository(readDbContext);
        var retrieved = await readRepository.GetByIdAsync(contact.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(companyId, retrieved.CompanyId);
        Assert.Equal("John", retrieved.FirstName);
        Assert.Equal("Smith", retrieved.LastName);
        Assert.Equal("john.smith@integration.org", retrieved.Email);
        Assert.Equal("Director", retrieved.Position);
        Assert.True(retrieved.IsDecisionMaker);
    }

    [Fact]
    public async Task SoftDeletedContact_ShouldBeFilteredOutByDefaultQueryFilter()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(SoftDeletedContact_ShouldBeFilteredOutByDefaultQueryFilter));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new ContactRepository(dbContext);

        var contact = Contact.Create(
            Guid.NewGuid(),
            "To Delete",
            "Contact",
            "delete@test.com",
            "",
            "",
            ""
        );

        await repository.AddAsync(contact);
        await dbContext.SaveChangesAsync();

        // Act: Soft Delete contact
        contact.SoftDelete(Guid.NewGuid());
        repository.Update(contact);
        await dbContext.SaveChangesAsync();

        // Assert: standard DbContext query should exclude soft deleted entity
        await using var queryDbContext = new ApplicationDbContext(options);
        var fetched = await queryDbContext.Set<Contact>().FirstOrDefaultAsync(c => c.Id == contact.Id);
        Assert.Null(fetched);

        // Assert: IgnoreQueryFilters should still find soft deleted entity
        var softDeleted = await queryDbContext.Set<Contact>().IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == contact.Id);
        Assert.NotNull(softDeleted);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetListAsync_ShouldFilterByCompanyIdAndSearchAndPaginate()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(GetListAsync_ShouldFilterByCompanyIdAndSearchAndPaginate));
        await using var dbContext = new ApplicationDbContext(options);
        var repository = new ContactRepository(dbContext);

        var company1 = Guid.NewGuid();
        var company2 = Guid.NewGuid();

        var ct1 = Contact.Create(company1, "Alpha", "One", "alpha1@test.com", "111", "Dev", "");
        var ct2 = Contact.Create(company1, "Beta", "Two", "beta2@test.com", "222", "QA", "");
        var ct3 = Contact.Create(company2, "Alpha", "Three", "alpha3@test.com", "333", "PM", "");

        await repository.AddAsync(ct1);
        await repository.AddAsync(ct2);
        await repository.AddAsync(ct3);
        await dbContext.SaveChangesAsync();

        // Act & Assert: Filter by companyId
        var company1Contacts = await repository.GetListAsync(1, 10, company1, search: null);
        Assert.Equal(2, company1Contacts.Count);

        // Act & Assert: Search by name
        var searchAlpha = await repository.GetListAsync(1, 10, companyId: null, search: "alpha");
        Assert.Equal(2, searchAlpha.Count);

        // Act & Assert: Count matching
        var totalCount = await repository.CountAsync(company1, search: null);
        Assert.Equal(2, totalCount);
    }
}
