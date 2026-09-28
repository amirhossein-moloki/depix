using BuildingBlocks.Common.Exceptions;
using Moq;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Application.Services;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Domain.Repositories;
using Xunit;

namespace Modules.Identity.Tests;

public class AuthUseCaseServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IPermissionRepository> _permissionRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IJwtTokenService> _jwtServiceMock = new();
    private readonly Mock<IPasswordHasherService> _hasherMock = new();
    private readonly Mock<IAuditEventLogger> _auditLoggerMock = new();

    private readonly AuthUseCaseService _service;

    public AuthUseCaseServiceTests()
    {
        _service = new AuthUseCaseService(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _permissionRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _jwtServiceMock.Object,
            _hasherMock.Object,
            _auditLoggerMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnLoginResult()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", "hashed_pwd");
        var role = Role.Create("Admin");
        var permission = Permission.Create("CRM.Lead.Create", "Create Lead", "CRM");

        _userRepoMock.Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("Password123!", "hashed_pwd"))
            .Returns(true);
        _roleRepoMock.Setup(r => r.GetRolesByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { role });
        _permissionRepoMock.Setup(p => p.GetPermissionsByRoleIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission> { permission });
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access_token");
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("raw_refresh_token");
        _jwtServiceMock.Setup(j => j.HashRefreshToken("raw_refresh_token")).Returns("hashed_refresh_token");

        // Act
        var result = await _service.LoginAsync(new LoginRequest("john@example.com", "Password123!"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("access_token", result.AccessToken);
        Assert.Equal("raw_refresh_token", result.RefreshToken);
        Assert.Equal(user.Id, result.UserId);
        Assert.Contains("Admin", result.Roles);
        Assert.Contains("CRM.Lead.Create", result.Permissions);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", "hashed_pwd");

        _userRepoMock.Setup(r => r.GetByEmailAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("WrongPassword", "hashed_pwd"))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.LoginAsync(new LoginRequest("john@example.com", "WrongPassword")));
    }
}
