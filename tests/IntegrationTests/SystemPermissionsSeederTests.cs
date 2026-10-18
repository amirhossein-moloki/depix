using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Infrastructure.Persistence;
using CrmErp.Host.Seeders;
using Modules.Identity.Domain.Entities;
using Xunit;

namespace IntegrationTests;

public class SystemPermissionsSeederTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        // Touch AssemblyReference to load entity configurations
        _ = Modules.Identity.Infrastructure.AssemblyReference.Assembly;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SeedAsync_ShouldSeedAllSystemPermissionsAndAttachToAdminRole()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();

        // Act
        await SystemPermissionsSeeder.SeedAsync(dbContext);

        // Assert
        var permissionsCount = await dbContext.Set<Permission>().CountAsync();
        Assert.Equal(SystemPermissionsSeeder.SystemPermissions.Length, permissionsCount);

        var adminRole = await dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name.ToLower() == "admin");

        Assert.NotNull(adminRole);
        Assert.Equal(permissionsCount, adminRole!.RolePermissions.Count);
    }

    [Fact]
    public async Task SeedAsync_WhenRunMultipleTimes_ShouldBeIdempotentWithoutDuplicates()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();

        // Act - Run 1
        await SystemPermissionsSeeder.SeedAsync(dbContext);

        var countAfterFirstRun = await dbContext.Set<Permission>().CountAsync();
        var adminRole = await dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name.ToLower() == "admin");
        var rolePermissionsAfterFirstRun = adminRole!.RolePermissions.Count;

        // Act - Run 2
        await SystemPermissionsSeeder.SeedAsync(dbContext);

        var countAfterSecondRun = await dbContext.Set<Permission>().CountAsync();
        var rolePermissionsAfterSecondRun = adminRole.RolePermissions.Count;

        // Assert
        Assert.Equal(countAfterFirstRun, countAfterSecondRun);
        Assert.Equal(rolePermissionsAfterFirstRun, rolePermissionsAfterSecondRun);
    }
}
