using Microsoft.EntityFrameworkCore;
using Modules.Platform.Domain.Entities;
using Modules.Platform.Domain.Repositories;

namespace Modules.Platform.Infrastructure.Persistence.Repositories;

public class AppSettingRepository : IAppSettingRepository
{
    private readonly DbContext _dbContext;

    public AppSettingRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppSetting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<AppSetting>()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim();
        return await _dbContext.Set<AppSetting>()
            .FirstOrDefaultAsync(s => s.Key == normalizedKey, cancellationToken);
    }

    public async Task<List<AppSetting>> GetListAsync(string? category = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<AppSetting>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(s => s.Category == category.Trim());
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsKeyAsync(string key, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedKey = key.Trim();
        var query = _dbContext.Set<AppSetting>().Where(s => s.Key == normalizedKey);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(AppSetting setting, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<AppSetting>().AddAsync(setting, cancellationToken);
    }

    public void Update(AppSetting setting)
    {
        _dbContext.Set<AppSetting>().Update(setting);
    }
}
