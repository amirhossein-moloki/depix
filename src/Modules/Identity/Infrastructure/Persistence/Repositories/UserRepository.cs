using Microsoft.EntityFrameworkCore;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Domain.Repositories;

namespace Modules.Identity.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbContext _dbContext;

    public UserRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<User>()
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<User>().AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _dbContext.Set<User>().Update(user);
    }
}
