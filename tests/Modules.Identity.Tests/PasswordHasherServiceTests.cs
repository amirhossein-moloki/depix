using Modules.Identity.Infrastructure.Services;
using Xunit;

namespace Modules.Identity.Tests;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _hasher = new();

    [Fact]
    public void HashPassword_And_VerifyPassword_ShouldSucceed()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _hasher.HashPassword(password);
        var isValid = _hasher.VerifyPassword(password, hash);
        var isInvalid = _hasher.VerifyPassword("WrongPassword", hash);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEqual(password, hash);
        Assert.True(isValid);
        Assert.False(isInvalid);
    }
}
