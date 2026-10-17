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
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AuthUseCaseService>> _loggerMock = new();

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
            _auditLoggerMock.Object,
            _loggerMock.Object
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

    [Fact]
    public async Task LoginAsync_PendingApprovalUser_ShouldThrowForbiddenException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "pending@example.com", "hashed_pwd", status: UserStatus.PendingApproval);

        _userRepoMock.Setup(r => r.GetByEmailAsync("pending@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("Password123!", "hashed_pwd"))
            .Returns(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.LoginAsync(new LoginRequest("pending@example.com", "Password123!")));

        Assert.Contains("pending administrator approval", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_RejectedUser_ShouldThrowForbiddenException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "rejected@example.com", "hashed_pwd", status: UserStatus.Rejected);

        _userRepoMock.Setup(r => r.GetByEmailAsync("rejected@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("Password123!", "hashed_pwd"))
            .Returns(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.LoginAsync(new LoginRequest("rejected@example.com", "Password123!")));

        Assert.Contains("rejected", ex.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidCurrentPassword_ShouldUpdatePasswordHashAndRevokeTokens()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", "old_hashed_pwd");
        var activeToken = RefreshToken.Create(user.Id, "hashed_token", TimeSpan.FromDays(7));
        user.AddRefreshToken(activeToken);

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("OldPassword123!", "old_hashed_pwd"))
            .Returns(true);
        _hasherMock.Setup(h => h.HashPassword("NewPassword123!"))
            .Returns("new_hashed_pwd");

        // Act
        await _service.ResetPasswordAsync(user.Id, new ResetPasswordRequest("OldPassword123!", "NewPassword123!"));

        // Assert
        Assert.Equal("new_hashed_pwd", user.PasswordHash);
        Assert.False(activeToken.IsActive);
        _userRepoMock.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _auditLoggerMock.Verify(a => a.LogAsync("PASSWORD_RESET_SUCCESS", user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_IncorrectCurrentPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com", "old_hashed_pwd");

        _userRepoMock.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("WrongPassword", "old_hashed_pwd"))
            .Returns(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _service.ResetPasswordAsync(user.Id, new ResetPasswordRequest("WrongPassword", "NewPassword123!")));

        Assert.Contains("Incorrect current password", ex.Message);
        _auditLoggerMock.Verify(a => a.LogAsync("PASSWORD_RESET_FAILURE", user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_EmptyPasswords_ShouldThrowValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ResetPasswordAsync(Guid.NewGuid(), new ResetPasswordRequest("", "NewPassword123!")));
    }

    [Fact]
    public async Task ResetUserPasswordByAdminAsync_ValidRequest_ShouldUpdatePasswordHashAndRevokeTokens()
    {
        // Arrange
        var targetUser = User.Create("Jane", "Doe", "jane@example.com", "old_hashed_pwd");
        var adminUserId = Guid.NewGuid();
        var activeToken = RefreshToken.Create(targetUser.Id, "hashed_token", TimeSpan.FromDays(7));
        targetUser.AddRefreshToken(activeToken);

        _userRepoMock.Setup(r => r.GetByIdAsync(targetUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(targetUser);
        _hasherMock.Setup(h => h.HashPassword("AdminNewPassword123!"))
            .Returns("admin_new_hashed_pwd");
        _roleRepoMock.Setup(r => r.GetRolesByUserIdAsync(targetUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>());

        // Act
        var result = await _service.ResetUserPasswordByAdminAsync(targetUser.Id, adminUserId, new AdminResetPasswordRequest("AdminNewPassword123!"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetUser.Id, result.Id);
        Assert.Equal("admin_new_hashed_pwd", targetUser.PasswordHash);
        Assert.False(activeToken.IsActive);
        _userRepoMock.Verify(r => r.UpdateAsync(targetUser, It.IsAny<CancellationToken>()), Times.Once);
        _auditLoggerMock.Verify(a => a.LogAsync("USER_PASSWORD_RESET_BY_ADMIN", targetUser.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetUserPasswordByAdminAsync_UserNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            _service.ResetUserPasswordByAdminAsync(targetUserId, Guid.NewGuid(), new AdminResetPasswordRequest("NewPassword123!")));
    }

    [Fact]
    public async Task ResetUserPasswordByAdminAsync_EmptyNewPassword_ShouldThrowValidationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ResetUserPasswordByAdminAsync(Guid.NewGuid(), Guid.NewGuid(), new AdminResetPasswordRequest("")));
    }
}
