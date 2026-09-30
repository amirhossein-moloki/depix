using Microsoft.EntityFrameworkCore;
using Xunit;
using Modules.Customer.Domain.Entities;
using Modules.Customer.Infrastructure.Persistence.Repositories;

namespace IntegrationTests;

public class CustomerEfInMemoryTests
{
    [Fact]
    public async Task CustomerRepository_CrudAndFilter_ShouldPersistCorrectly()
    {
        var options = new DbContextOptionsBuilder<TestCustomerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new TestCustomerDbContext(options);
        var repository = new CustomerRepository(dbContext);

        var companyId1 = Guid.NewGuid();
        var companyId2 = Guid.NewGuid();

        var customer1 = Customer.Create(companyId1, "CUST-001", notes: "Alpha Corp customer");
        var customer2 = Customer.Create(companyId2, "CUST-002", notes: "Beta Corp customer");

        await repository.AddAsync(customer1);
        await repository.AddAsync(customer2);
        await dbContext.SaveChangesAsync();

        // Query by Company
        var retrievedByCompany = await repository.GetByCompanyIdAsync(companyId1);
        Assert.NotNull(retrievedByCompany);
        Assert.Equal("CUST-001", retrievedByCompany.CustomerNumber);

        // Query by Customer Number
        var retrievedByNumber = await repository.GetByCustomerNumberAsync("cust-002");
        Assert.NotNull(retrievedByNumber);
        Assert.Equal(companyId2, retrievedByNumber.CompanyId);

        // Paged Search
        var (items, count) = await repository.GetPagedAsync(1, 10, search: "alpha");
        Assert.Equal(1, count);
        Assert.Single(items);
        Assert.Equal("CUST-001", items[0].CustomerNumber);

        // Soft Delete and Exists Check
        customer1.Archive();
        repository.Update(customer1);
        await dbContext.SaveChangesAsync();

        var existsAfterArchive = await repository.ExistsForCompanyAsync(companyId1);
        Assert.False(existsAfterArchive);
    }
}

public class TestCustomerDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();

    public TestCustomerDbContext(DbContextOptions<TestCustomerDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Modules.Customer.Infrastructure.AssemblyReference).Assembly);
    }
}
