using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Application.Mappings;
using Modules.Platform.Domain.Entities;
using Modules.Platform.Domain.Repositories;
using Modules.Platform.Domain.Services;

namespace Modules.Platform.Application.Commands;

public class UploadFileCommandHandler : ICommandHandler<UploadFileCommand, FileAssetDto>
{
    private readonly IFileAssetRepository _repository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public UploadFileCommandHandler(
        IFileAssetRepository repository,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<FileAssetDto> HandleAsync(UploadFileCommand command, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(command.FileName)?.ToLowerInvariant() ?? string.Empty;
        var storageKey = await _fileStorage.SaveAsync(command.ContentStream, command.FileName, command.ContentType, cancellationToken);

        var fileAsset = FileAsset.Create(
            command.EntityType,
            command.EntityId,
            command.FileName,
            command.OriginalFileName,
            command.ContentType,
            extension,
            command.Size,
            storageKey,
            "LOCAL",
            command.Description,
            command.UploadedBy);

        await _repository.AddAsync(fileAsset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return fileAsset.ToDto();
    }
}

public class DeleteFileCommandHandler : ICommandHandler<DeleteFileCommand>
{
    private readonly IFileAssetRepository _repository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFileCommandHandler(
        IFileAssetRepository repository,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(DeleteFileCommand command, CancellationToken cancellationToken = default)
    {
        var fileAsset = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (fileAsset == null)
        {
            throw new EntityNotFoundException("FileAsset", command.Id);
        }

        fileAsset.SoftDelete(command.DeletedBy);
        _repository.Update(fileAsset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _fileStorage.DeleteAsync(fileAsset.StorageKey, cancellationToken);
    }
}
