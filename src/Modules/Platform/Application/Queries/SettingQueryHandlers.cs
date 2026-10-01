using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Application.Mappings;
using Modules.Platform.Domain.Repositories;

namespace Modules.Platform.Application.Queries;

public class GetSettingsQueryHandler : IQueryHandler<GetSettingsQuery, List<AppSettingDto>>
{
    private readonly IAppSettingRepository _repository;

    public GetSettingsQueryHandler(IAppSettingRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AppSettingDto>> HandleAsync(GetSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var settings = await _repository.GetListAsync(query.Category, query.IsActive, cancellationToken);
        return settings.Select(s => s.ToDto()).ToList();
    }
}

public class GetSettingByKeyQueryHandler : IQueryHandler<GetSettingByKeyQuery, AppSettingDto>
{
    private readonly IAppSettingRepository _repository;

    public GetSettingByKeyQueryHandler(IAppSettingRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppSettingDto> HandleAsync(GetSettingByKeyQuery query, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByKeyAsync(query.Key, cancellationToken);
        if (setting == null)
        {
            throw new EntityNotFoundException("AppSetting", query.Key);
        }

        return setting.ToDto();
    }
}
