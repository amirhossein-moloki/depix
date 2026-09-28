using Microsoft.AspNetCore.Identity;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Domain.Entities;

namespace Modules.Identity.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(string password)
    {
        // Dummy User instance for PasswordHasher<User> API requirements
        var dummyUser = User.Create("dummy", "dummy", "dummy@example.com", "hash");
        return _hasher.HashPassword(dummyUser, password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var dummyUser = User.Create("dummy", "dummy", "dummy@example.com", "hash");
        var result = _hasher.VerifyHashedPassword(dummyUser, hashedPassword, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
