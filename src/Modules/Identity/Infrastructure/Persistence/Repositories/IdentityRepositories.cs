using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Domain.Repositories;

namespace Modules.Identity.Infrastructure.Persistence.Repositories;

public class IdentityRepositories : IUserRepository, IRoleRepository, IPermissionRepository, IRefreshTokenRepository
{
    private readonly DbContext _dbContext;

    public IdentityRepositories(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // IUserRepository & IRoleRepository GetByIdAsync
    async Task<User?> IUserRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    async Task<Role?> IRoleRepository.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetPendingUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .Where(u => u.Status == UserStatus.PendingApproval)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<User>().AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var entry = _dbContext.Entry(user);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Attach(user);
            entry.State = EntityState.Modified;
        }

        foreach (var refreshToken in user.RefreshTokens)
        {
            var tokenEntry = _dbContext.Entry(refreshToken);
            if (tokenEntry.State == EntityState.Detached)
            {
                _dbContext.Set<RefreshToken>().Add(refreshToken);
            }
            else if (tokenEntry.State == EntityState.Modified)
            {
                tokenEntry.State = EntityState.Added;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        var userRoles = await _dbContext.Set<UserRole>().Where(ur => ur.UserId == user.Id).ToListAsync(cancellationToken);
        _dbContext.Set<UserRole>().RemoveRange(userRoles);

        var refreshTokens = await _dbContext.Set<RefreshToken>().Where(rt => rt.UserId == user.Id).ToListAsync(cancellationToken);
        _dbContext.Set<RefreshToken>().RemoveRange(refreshTokens);

        _dbContext.Set<User>().Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // IRoleRepository
    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<List<Role>> GetRolesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _dbContext.Set<UserRole>()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        return await _dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
            .Where(r => userRoles.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    async Task<List<Role>> IRoleRepository.GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Role>()
            .Include(r => r.RolePermissions)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Role>().AddAsync(role, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var existingPerms = await _dbContext.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _dbContext.Set<RolePermission>().RemoveRange(existingPerms);

        var newPerms = permissionIds.Distinct().Select(pid => new RolePermission(roleId, pid));
        await _dbContext.Set<RolePermission>().AddRangeAsync(newPerms, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    // IPermissionRepository
    public async Task<List<Permission>> GetPermissionsByRoleIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var permissionIds = await _dbContext.Set<RolePermission>()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _dbContext.Set<Permission>()
            .Where(p => permissionIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    async Task<List<Permission>> IPermissionRepository.GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Set<Permission>().ToListAsync(cancellationToken);
    }

    // IRefreshTokenRepository
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        var entry = _dbContext.Entry(refreshToken);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Set<RefreshToken>().Update(refreshToken);
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
