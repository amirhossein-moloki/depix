using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Domain.Repositories;

public interface IFileAssetRepository
{
    Task<FileAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<FileAsset>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task AddAsync(FileAsset fileAsset, CancellationToken cancellationToken = default);
    void Update(FileAsset fileAsset);
}
