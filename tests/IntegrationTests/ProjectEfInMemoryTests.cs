using Microsoft.EntityFrameworkCore;
using Xunit;
using Modules.Project.Domain.Entities;
using Modules.Project.Infrastructure.Persistence.Repositories;
using Modules.Customer.Domain.Entities;
using Modules.Customer.Infrastructure.Persistence.Repositories;

namespace IntegrationTests;

public class ProjectEfInMemoryTests
{
    [Fact]
    public async Task ProjectRepository_FullLifecycleAndRelationships_ShouldPersistAndFilterCorrectly()
    {
        var options = new DbContextOptionsBuilder<TestProjectDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Guid companyId = Guid.NewGuid();
        Guid customerId;
        Guid projectId;

        // 1. Prepare Customer
        await using (var dbContext = new TestProjectDbContext(options))
        {
            var customerRepo = new CustomerRepository(dbContext);
            var customer = Customer.Create(companyId, "CUST-PROJ-001");
            await customerRepo.AddAsync(customer);
            await dbContext.SaveChangesAsync();
            customerId = customer.Id;
        }

        // 2. Create Project for Customer
        await using (var dbContext = new TestProjectDbContext(options))
        {
            var projectRepo = new ProjectRepository(dbContext);
            var project = Project.Create(
                customerId,
                "Corporate Web Portal",
                type: "WebDevelopment",
                companyId: companyId,
                description: "Main corporate web portal redesign",
                startDate: new DateOnly(2026, 10, 1),
                plannedDeliveryDate: new DateOnly(2026, 12, 15),
                notes: "Phase 8 integration test");

            await projectRepo.AddAsync(project);
            await dbContext.SaveChangesAsync();
            projectId = project.Id;
        }

        // 3. Retrieve Project & Verify Initial State
        await using (var dbContext = new TestProjectDbContext(options))
        {
            var projectRepo = new ProjectRepository(dbContext);
            var retrieved = await projectRepo.GetByIdAsync(projectId);
            Assert.NotNull(retrieved);
            Assert.Equal(customerId, retrieved.CustomerId);
            Assert.Equal("Corporate Web Portal", retrieved.Name);
            Assert.Equal("PLANNED", retrieved.Status);
        }

        // 4. Start Project and Add Repository, Deployment, Requirement Metadata
        await using (var dbContext = new TestProjectDbContext(options))
        {
            var projectRepo = new ProjectRepository(dbContext);
            var retrieved = await projectRepo.GetByIdAsync(projectId);
            Assert.NotNull(retrieved);

            retrieved.Start(new DateOnly(2026, 10, 5));
            retrieved.AddRepository("GitHub", "https://github.com/company/web-portal.git", "Web Portal App", "main", "Frontend + Backend repo");
            retrieved.AddDeployment("Production", "server-prod-01", "AWS", "portal.company.com", "Active", "1.0.0", "Live production URL");
            retrieved.SetRequirement("Increase customer conversion", "Auth, Dashboard, Payments", ".NET 8, React", "B2B Clients");

            projectRepo.Update(retrieved);
            await dbContext.SaveChangesAsync();
        }

        // 5. Query Project with Details
        await using (var dbContext = new TestProjectDbContext(options))
        {
            var projectRepo = new ProjectRepository(dbContext);
            var updatedProject = await projectRepo.GetByIdAsync(projectId);
            Assert.NotNull(updatedProject);
            Assert.Equal("IN_PROGRESS", updatedProject.Status);
            Assert.Equal(new DateOnly(2026, 10, 5), updatedProject.StartDate);
            Assert.Single(updatedProject.Repositories);
            Assert.Single(updatedProject.Deployments);
            Assert.NotNull(updatedProject.Requirement);
            Assert.Equal("https://github.com/company/web-portal.git", updatedProject.Repositories.First().Url);
            Assert.Equal("portal.company.com", updatedProject.Deployments.First().Domain);
            Assert.Equal("Increase customer conversion", updatedProject.Requirement.BusinessGoal);
        }

        // 6. Paged Search & Filter by Customer
        await using (var dbContext = new TestProjectDbContext(options))
        {
            var projectRepo = new ProjectRepository(dbContext);
            var (items, count) = await projectRepo.GetPagedAsync(
                page: 1,
                pageSize: 10,
                search: "Corporate",
                status: "IN_PROGRESS",
                customerId: customerId);

            Assert.Equal(1, count);
            Assert.Single(items);
            Assert.Equal("Corporate Web Portal", items[0].Name);
        }

        // 7. Complete Project
        await using (var dbContext = new TestProjectDbContext(options))
        {
            var projectRepo = new ProjectRepository(dbContext);
            var projectToComplete = await projectRepo.GetByIdAsync(projectId);
            Assert.NotNull(projectToComplete);

            projectToComplete.Complete(new DateOnly(2026, 12, 10));
            projectRepo.Update(projectToComplete);
            await dbContext.SaveChangesAsync();
        }

        // 8. Soft Delete / Archive
        await using (var dbContext = new TestProjectDbContext(options))
        {
            var projectRepo = new ProjectRepository(dbContext);
            var completedProject = await projectRepo.GetByIdAsync(projectId);
            Assert.NotNull(completedProject);
            Assert.Equal("COMPLETED", completedProject.Status);
            Assert.Equal(new DateOnly(2026, 12, 10), completedProject.ActualDeliveryDate);

            completedProject.Archive();
            projectRepo.Update(completedProject);
            await dbContext.SaveChangesAsync();
        }

        // 9. Verify Customer & Company were not duplicated
        await using (var dbContext = new TestProjectDbContext(options))
        {
            var customerCount = await dbContext.Set<Customer>().CountAsync();
            Assert.Equal(1, customerCount);
        }
    }
}

public class TestProjectDbContext : DbContext
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Customer> Customers => Set<Customer>();

    public TestProjectDbContext(DbContextOptions<TestProjectDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Modules.Project.Infrastructure.AssemblyReference).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Modules.Customer.Infrastructure.AssemblyReference).Assembly);
    }
}
