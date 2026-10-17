using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Domain.Repositories;

namespace Modules.Identity.Infrastructure.Persistence.Repositories;

public class IdentityRepositories : IUserRepository, IUserApprovalRepository, IRoleRepository, IPermissionRepository, IRefreshTokenRepository
{
    private readonly DbContext _dbContext;

    public IdentityRepositories(DbContext dbContext) => _dbContext = dbContext;

    async Task<User?> IUserRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _dbContext.Set<User>().Include(u => u.UserRoles).Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    async Task<Role?> IRoleRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _dbContext.Set<Role>().Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _dbContext.Set<User>().Include(u => u.UserRoles).Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<User>().AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRegistrationAsync(User user, UserApproval approval, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.Set<User>().AddAsync(user, cancellationToken);
        await _dbContext.Set<UserApproval>().AddAsync(approval, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<User>().Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserApproval?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.Set<UserApproval>().FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public async Task<List<UserApproval>> GetPendingAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<UserApproval>().Where(x => x.Status == UserApprovalStatus.Pending)
            .OrderBy(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(UserApproval approval, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<UserApproval>().AddAsync(approval, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserApproval approval, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<UserApproval>().Update(approval);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _dbContext.Set<Role>().FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower(), cancellationToken);

    public async Task<List<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var roleIds = await _dbContext.Set<UserRole>().Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync(cancellationToken);
        return await _dbContext.Set<Role>().Where(r => roleIds.Contains(r.Id)).ToListAsync(cancellationToken);
    }

    async Task<List<Role>> IRoleRepository.GetAllAsync(CancellationToken cancellationToken)
        => await _dbContext.Set<Role>().ToListAsync(cancellationToken);

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Role>().AddAsync(role, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Permission>> GetPermissionsByRoleIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var ids = roleIds.ToArray();
        var permissionIds = await _dbContext.Set<RolePermission>().Where(rp => ids.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId).Distinct().ToListAsync(cancellationToken);
        return await _dbContext.Set<Permission>().Where(p => permissionIds.Contains(p.Id)).ToListAsync(cancellationToken);
    }

    async Task<List<Permission>> IPermissionRepository.GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<Permission>().ToListAsync(cancellationToken);

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => await _dbContext.Set<RefreshToken>().FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<RefreshToken>().Update(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}