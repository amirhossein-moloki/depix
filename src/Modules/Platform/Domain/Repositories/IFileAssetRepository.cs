using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Domain.Repositories;

public interface IFileAssetRepository
{
    Task<FileAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(FileAsset fileAsset, CancellationToken cancellationToken = default);
}
