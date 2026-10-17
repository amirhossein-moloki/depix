using BuildingBlocks.Common.Exceptions;
using Moq;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Application.Contracts;
using Modules.Identity.Application.Services;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Domain.Repositories;
using Xunit;

namespace Modules.Identity.Tests;

public class UserApprovalAndManagementTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IPermissionRepository> _permissionRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IJwtTokenService> _jwtServiceMock = new();
    private readonly Mock<IPasswordHasherService> _hasherMock = new();
    private readonly Mock<IAuditEventLogger> _auditLoggerMock = new();

    private readonly AuthUseCaseService _authService;
    private readonly UserManagementService _userManagementService;

    public UserApprovalAndManagementTests()
    {
        _authService = new AuthUseCaseService(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _permissionRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _jwtServiceMock.Object,
            _hasherMock.Object,
            _auditLoggerMock.Object
        );

        _userManagementService = new UserManagementService(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _hasherMock.Object,
            _auditLoggerMock.Object
        );
    }

    [Fact]
    public void UserDomain_CreatePending_ShouldHavePendingApprovalStatus()
    {
        var user = User.CreatePending("Pending", "User", "pending@example.com", "hashed_pwd");

        Assert.Equal(UserStatus.PendingApproval, user.Status);
        Assert.Null(user.ApprovedBy);
        Assert.Null(user.ApprovedAt);
    }

    [Fact]
    public void UserDomain_Approve_ShouldSetStatusToActive()
    {
        var user = User.CreatePending("Pending", "User", "pending@example.com", "hashed_pwd");
        var adminId = Guid.NewGuid();

        user.Approve(adminId);

        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(adminId, user.ApprovedBy);
        Assert.NotNull(user.ApprovedAt);
        Assert.Null(user.RejectionReason);
    }

    [Fact]
    public void UserDomain_Reject_ShouldSetStatusToRejectedAndRecordReason()
    {
        var user = User.CreatePending("Pending", "User", "pending@example.com", "hashed_pwd");
        var adminId = Guid.NewGuid();

        user.Reject(adminId, "Incomplete verification documents");

        Assert.Equal(UserStatus.Rejected, user.Status);
        Assert.Equal(adminId, user.ApprovedBy);
        Assert.NotNull(user.ApprovedAt);
        Assert.Equal("Incomplete verification documents", user.RejectionReason);
    }

    [Fact]
    public async Task LoginAsync_PendingApprovalUser_ShouldThrowUnauthorizedException()
    {
        var user = User.CreatePending("Pending", "User", "pending@example.com", "hashed_pwd");

        _userRepoMock.Setup(r => r.GetByEmailAsync("pending@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("Password123!", "hashed_pwd"))
            .Returns(true);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _authService.LoginAsync(new LoginRequest("pending@example.com", "Password123!")));
    }

    [Fact]
    public async Task LoginAsync_RejectedUser_ShouldThrowUnauthorizedException()
    {
        var user = User.CreatePending("Rejected", "User", "rejected@example.com", "hashed_pwd");
        user.Reject(Guid.NewGuid(), "Rejected by admin");

        _userRepoMock.Setup(r => r.GetByEmailAsync("rejected@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("Password123!", "hashed_pwd"))
            .Returns(true);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _authService.LoginAsync(new LoginRequest("rejected@example.com", "Password123!")));
    }

    [Fact]
    public async Task RegisterPublicAsync_ValidRequest_ShouldCreateUserWithPendingApprovalStatus()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("newuser@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.HashPassword("UserPassword123!"))
            .Returns("hashed_pwd");
        _roleRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { Role.Create("SalesPerson") });

        var result = await _userManagementService.RegisterPublicAsync(
            new RegisterRequest("New", "User", "newuser@example.com", "UserPassword123!", "123456789"));

        Assert.NotNull(result);
        Assert.Equal("newuser@example.com", result.Email);
        Assert.Equal(UserStatus.PendingApproval.ToString(), result.Status);
        _userRepoMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Status == UserStatus.PendingApproval), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserByAdminAsync_ShouldCreateActiveUserAndAutoApprove()
    {
        var adminId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByEmailAsync("admincreated@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.HashPassword("AdminCreatedPwd123!"))
            .Returns("hashed_pwd");
        _roleRepoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role> { Role.Create("SalesPerson") });

        var result = await _userManagementService.CreateUserByAdminAsync(
            new CreateUserByAdminRequest("Admin", "Created", "admincreated@example.com", "AdminCreatedPwd123!", null, "SalesPerson"),
            adminId);

        Assert.NotNull(result);
        Assert.Equal(UserStatus.Active.ToString(), result.Status);
        Assert.Equal(adminId, result.ApprovedBy);
    }
}
