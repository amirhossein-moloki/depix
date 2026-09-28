using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Modules.Identity.Infrastructure.Authorization;
using Xunit;

namespace Modules.Identity.Tests;

public class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _handler = new();

    [Fact]
    public void HandleAsync_UserHasPermission_ShouldSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("CRM.Lead.Create");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("permissions", "CRM.Lead.Create"),
            new Claim("permissions", "CRM.Lead.Read")
        }, "TestAuth"));

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        _handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public void HandleAsync_UserDoesNotHavePermission_ShouldNotSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("CRM.Lead.Delete");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("permissions", "CRM.Lead.Create"),
            new Claim("permissions", "CRM.Lead.Read")
        }, "TestAuth"));

        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        // Act
        _handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }
}
