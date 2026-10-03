using System.Security.Claims;
using Microsoft.Extensions.Options;
using Modules.Identity.Infrastructure.Options;
using Modules.Identity.Infrastructure.Services;
using Xunit;

namespace Modules.Identity.Tests;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        var jwtOptions = new JwtOptions
        {
            Secret = "Test_Jwt_Secret_Key_32_Bytes_Long_Minimum_For_Unit_Tests!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        _service = new JwtTokenService(Options.Create(jwtOptions));
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Admin", "User" };
        var permissions = new[] { "CRM.Lead.Read", "CRM.Lead.Create" };

        // Act
        var token = _service.GenerateAccessToken(userId, email, roles, permissions);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token); // JWT structure check (header.payload.signature)
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Act
        var refreshToken = _service.GenerateRefreshToken();

        // Assert
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken);
    }

    [Fact]
    public void HashRefreshToken_ShouldReturnConsistentHash()
    {
        // Arrange
        var rawToken = "sample-refresh-token";

        // Act
        var hash1 = _service.HashRefreshToken(rawToken);
        var hash2 = _service.HashRefreshToken(rawToken);

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.NotEqual(rawToken, hash1);
    }
}
