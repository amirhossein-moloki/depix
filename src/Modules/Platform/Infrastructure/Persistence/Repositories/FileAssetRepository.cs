using Microsoft.EntityFrameworkCore;
using Modules.Platform.Domain.Entities;
using Modules.Platform.Domain.Repositories;

namespace Modules.Platform.Infrastructure.Persistence.Repositories;

public class FileAssetRepository : IFileAssetRepository
{
    private readonly DbContext _dbContext;

    public FileAssetRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FileAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<FileAsset>()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<List<FileAsset>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<FileAsset>()
            .Where(f => f.EntityType == entityType && f.EntityId == entityId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FileAsset fileAsset, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<FileAsset>().AddAsync(fileAsset, cancellationToken);
    }

    public void Update(FileAsset fileAsset)
    {
        _dbContext.Set<FileAsset>().Update(fileAsset);
    }
}
