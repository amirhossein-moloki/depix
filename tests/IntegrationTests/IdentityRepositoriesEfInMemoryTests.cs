using BuildingBlocks.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Application.Services;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Infrastructure.Persistence.Repositories;
using NSubstitute;
using Xunit;

namespace IntegrationTests;

public class IdentityRepositoriesEfInMemoryTests
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
    public async Task LoginAsync_WithTrackedUserAndRefreshToken_ShouldNotThrowDbUpdateConcurrencyException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        await using var setupContext = new TestIdentityDbContext(options);
        var passwordHash = "hashed_secret_password";
        var user = User.Create("John", "Doe", "john.doe@example.com", passwordHash);
        var role = Role.Create("User");
        user.AddRole(role.Id);

        setupContext.Set<User>().Add(user);
        setupContext.Set<Role>().Add(role);
        await setupContext.SaveChangesAsync();

        await using var dbContext = new TestIdentityDbContext(options);
        var identityRepo = new IdentityRepositories(dbContext);

        var hasher = Substitute.For<IPasswordHasherService>();
        hasher.VerifyPassword("Password123!", passwordHash).Returns(true);

        var jwt = Substitute.For<IJwtTokenService>();
        jwt.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<IEnumerable<string>>())
            .Returns("sample_access_token");
        jwt.GenerateRefreshToken().Returns("raw_refresh_token_1");
        jwt.HashRefreshToken("raw_refresh_token_1").Returns("hashed_refresh_token_1");

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

        // Act - First Login
        var result1 = await authService.LoginAsync(new LoginRequest("john.doe@example.com", "Password123!"), "127.0.0.1");

        // Assert - First Login
        Assert.NotNull(result1);
        Assert.Equal("sample_access_token", result1.AccessToken);

        // Verify stored state in DbContext
        var storedUser = await dbContext.Set<User>()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        Assert.NotNull(storedUser);
        Assert.NotNull(storedUser.LastLoginAt);
        Assert.Single(storedUser.RefreshTokens);
        Assert.Equal("hashed_refresh_token_1", storedUser.RefreshTokens.First().TokenHash);

        // Act - Second Login with existing refresh tokens present in tracked aggregate graph
        jwt.GenerateRefreshToken().Returns("raw_refresh_token_2");
        jwt.HashRefreshToken("raw_refresh_token_2").Returns("hashed_refresh_token_2");

        var result2 = await authService.LoginAsync(new LoginRequest("john.doe@example.com", "Password123!"), "127.0.0.1");

        // Assert - Second Login
        Assert.NotNull(result2);
        var reFetchedUser = await dbContext.Set<User>()
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        Assert.NotNull(reFetchedUser);
        Assert.Equal(2, reFetchedUser.RefreshTokens.Count);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ShouldThrowUnauthorizedExceptionAndNotUpdateLastLoginAt()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TestIdentityDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        await using var setupContext = new TestIdentityDbContext(options);
        var passwordHash = "hashed_secret_password";
        var user = User.Create("Jane", "Doe", "jane.doe@example.com", passwordHash);
        setupContext.Set<User>().Add(user);
        await setupContext.SaveChangesAsync();

        await using var dbContext = new TestIdentityDbContext(options);
        var identityRepo = new IdentityRepositories(dbContext);

        var hasher = Substitute.For<IPasswordHasherService>();
        hasher.VerifyPassword("WrongPassword!", passwordHash).Returns(false);

        var jwt = Substitute.For<IJwtTokenService>();
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

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            authService.LoginAsync(new LoginRequest("jane.doe@example.com", "WrongPassword!"), "127.0.0.1"));

        var reFetchedUser = await dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        Assert.NotNull(reFetchedUser);
        Assert.Null(reFetchedUser.LastLoginAt);
        Assert.Empty(reFetchedUser.RefreshTokens);
    }
}
