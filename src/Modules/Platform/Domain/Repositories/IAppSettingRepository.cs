using Modules.Platform.Domain.Entities;

namespace Modules.Platform.Domain.Repositories;

public interface IAppSettingRepository
{
    Task<AppSetting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<List<AppSetting>> GetListAsync(string? category = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsKeyAsync(string key, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(AppSetting setting, CancellationToken cancellationToken = default);
    void Update(AppSetting setting);
}
