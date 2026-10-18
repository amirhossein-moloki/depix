using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Application.Services;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Infrastructure.Persistence;
using Modules.Identity.Infrastructure.Persistence.Repositories;
using NSubstitute;
using Xunit;

namespace IntegrationTests;

public class IdentityDataSeederIntegrationTests
{
    private class TestIdentityDbContext : DbContext
    {
        public TestIdentityDbContext(DbContextOptions<TestIdentityDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Modules.Identity.Infrastructure.AssemblyReference).Assembly);
        }
    }

    [Fact]
    public async Task SeedPermissionsAndRolesAsync_ShouldPopulatePermissionsAndAdminRole_AndBeIdempotent()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        await using var dbContext = new ApplicationDbContext(options);

        // Act - First Seeding
        await IdentityDataSeeder.SeedPermissionsAndRolesAsync(dbContext);

        // Assert - First Seeding
        var permissionCount = await dbContext.Set<Permission>().CountAsync();
        var adminRole = await dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name.ToLower() == "admin");

        Assert.Equal(IdentityDataSeeder.SystemPermissions.Length, permissionCount);
        Assert.NotNull(adminRole);
        Assert.Equal(IdentityDataSeeder.SystemPermissions.Length, adminRole.RolePermissions.Count);

        var rolePermissionCount = await dbContext.Set<RolePermission>().CountAsync();
        Assert.Equal(IdentityDataSeeder.SystemPermissions.Length, rolePermissionCount);

        // Act - Second Seeding (Idempotency Check)
        await IdentityDataSeeder.SeedPermissionsAndRolesAsync(dbContext);

        // Assert - Second Seeding
        var permissionCountAfterSecondSeed = await dbContext.Set<Permission>().CountAsync();
        var rolePermissionCountAfterSecondSeed = await dbContext.Set<RolePermission>().CountAsync();

        Assert.Equal(IdentityDataSeeder.SystemPermissions.Length, permissionCountAfterSecondSeed);
        Assert.Equal(IdentityDataSeeder.SystemPermissions.Length, rolePermissionCountAfterSecondSeed);
    }

    [Fact]
    public async Task AdminUserLogin_AfterSeeding_ShouldReceiveAllPermissionCodes()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        await using var setupContext = new ApplicationDbContext(options);
        await IdentityDataSeeder.SeedPermissionsAndRolesAsync(setupContext);

        var adminRole = await setupContext.Set<Role>().FirstAsync(r => r.Name.ToLower() == "admin");
        var passwordHash = "hashed_admin_password";
        var adminUser = User.Create("Admin", "System", "moazzenhadi46@gmail.com", passwordHash, status: UserStatus.Active);
        adminUser.AddRole(adminRole.Id);

        setupContext.Set<User>().Add(adminUser);
        await setupContext.SaveChangesAsync();

        await using var dbContext = new ApplicationDbContext(options);
        var identityRepo = new IdentityRepositories(dbContext);

        var hasher = Substitute.For<IPasswordHasherService>();
        hasher.VerifyPassword("AdminSecret123!", passwordHash).Returns(true);

        var jwt = Substitute.For<IJwtTokenService>();
        jwt.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>())
            .Returns("admin_access_token");
        jwt.GenerateRefreshToken().Returns("raw_refresh_token");
        jwt.HashRefreshToken("raw_refresh_token").Returns("hashed_refresh_token");

        var auditLogger = Substitute.For<IAuditEventLogger>();
        var logger = Substitute.For<Microsoft.Extensions.Logging.ILogger<AuthUseCaseService>>();

        var authService = new AuthUseCaseService(
            identityRepo,
            identityRepo,
            identityRepo,
            identityRepo,
            jwt,
            hasher,
            auditLogger,
            logger
        );

        // Act
        var loginResult = await authService.LoginAsync(new LoginRequest("moazzenhadi46@gmail.com", "AdminSecret123!"), "127.0.0.1");

        // Assert
        Assert.NotNull(loginResult);
        Assert.Contains("Admin", loginResult.Roles);
        Assert.Equal(IdentityDataSeeder.SystemPermissions.Length, loginResult.Permissions.Count);
        Assert.Contains("Reporting.Read", loginResult.Permissions);
        Assert.Contains("CRM.Lead.Read", loginResult.Permissions);
        Assert.Contains("Support.Ticket.Read", loginResult.Permissions);
    }
}
