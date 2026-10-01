using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Application.Mappings;
using Modules.Platform.Domain.Entities;
using Modules.Platform.Domain.Repositories;

namespace Modules.Platform.Application.Commands;

public class CreateSettingCommandHandler : ICommandHandler<CreateSettingCommand, AppSettingDto>
{
    private readonly IAppSettingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSettingCommandHandler(IAppSettingRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AppSettingDto> HandleAsync(CreateSettingCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsKeyAsync(command.Key, null, cancellationToken);
        if (exists)
        {
            throw new BusinessRuleException($"Setting with key '{command.Key}' already exists.");
        }

        var setting = AppSetting.Create(
            command.Key,
            command.Value,
            command.Description,
            command.Category,
            command.IsActive);

        await _repository.AddAsync(setting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return setting.ToDto();
    }
}

public class UpdateSettingCommandHandler : ICommandHandler<UpdateSettingCommand, AppSettingDto>
{
    private readonly IAppSettingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSettingCommandHandler(IAppSettingRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AppSettingDto> HandleAsync(UpdateSettingCommand command, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByKeyAsync(command.Key, cancellationToken);
        if (setting == null)
        {
            throw new EntityNotFoundException("AppSetting", command.Key);
        }

        setting.Update(command.Value, command.Description, command.Category, command.IsActive);
        _repository.Update(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return setting.ToDto();
    }
}

public class DeleteSettingCommandHandler : ICommandHandler<DeleteSettingCommand>
{
    private readonly IAppSettingRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSettingCommandHandler(IAppSettingRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeleteSettingCommand command, CancellationToken cancellationToken = default)
    {
        var setting = await _repository.GetByKeyAsync(command.Key, cancellationToken);
        if (setting == null)
        {
            throw new EntityNotFoundException("AppSetting", command.Key);
        }

        setting.SoftDelete(command.DeletedBy);
        _repository.Update(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
