using BuildingBlocks.Domain.Models;

namespace Modules.Identity.Domain.Entities;

public class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? CreatedByIp { get; private set; }

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

    private RefreshToken() { }

    public RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiresAt, DateTime createdAt, string? createdByIp = null)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        CreatedByIp = createdByIp;
    }

    public static RefreshToken Create(Guid userId, string tokenHash, TimeSpan expiresIn, string? createdByIp = null)
    {
        var now = DateTime.UtcNow;
        return new RefreshToken(Guid.NewGuid(), userId, tokenHash, now.Add(expiresIn), now, createdByIp);
    }

    public void Revoke()
    {
        if (RevokedAt == null)
        {
            RevokedAt = DateTime.UtcNow;
        }
    }
}
