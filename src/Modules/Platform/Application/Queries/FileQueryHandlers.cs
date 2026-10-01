using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Application.Mappings;
using Modules.Platform.Domain.Repositories;
using Modules.Platform.Domain.Services;

namespace Modules.Platform.Application.Queries;

public class GetFileMetadataQueryHandler : IQueryHandler<GetFileMetadataQuery, FileAssetDto>
{
    private readonly IFileAssetRepository _repository;

    public GetFileMetadataQueryHandler(IFileAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<FileAssetDto> HandleAsync(GetFileMetadataQuery query, CancellationToken cancellationToken = default)
    {
        var fileAsset = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (fileAsset == null)
        {
            throw new EntityNotFoundException("FileAsset", query.Id);
        }

        return fileAsset.ToDto();
    }
}

public class GetFilesByEntityQueryHandler : IQueryHandler<GetFilesByEntityQuery, List<FileAssetDto>>
{
    private readonly IFileAssetRepository _repository;

    public GetFilesByEntityQueryHandler(IFileAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<FileAssetDto>> HandleAsync(GetFilesByEntityQuery query, CancellationToken cancellationToken = default)
    {
        var files = await _repository.GetByEntityAsync(query.EntityType, query.EntityId, cancellationToken);
        return files.Select(f => f.ToDto()).ToList();
    }
}

public class DownloadFileQueryHandler : IQueryHandler<DownloadFileQuery, FileDownloadDto>
{
    private readonly IFileAssetRepository _repository;
    private readonly IFileStorage _fileStorage;

    public DownloadFileQueryHandler(
        IFileAssetRepository repository,
        IFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<FileDownloadDto> HandleAsync(DownloadFileQuery query, CancellationToken cancellationToken = default)
    {
        var fileAsset = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (fileAsset == null)
        {
            throw new EntityNotFoundException("FileAsset", query.Id);
        }

        var result = await _fileStorage.GetAsync(fileAsset.StorageKey, cancellationToken);
        if (result == null)
        {
            throw new EntityNotFoundException("FileContent", fileAsset.StorageKey);
        }

        return new FileDownloadDto(
            result.Value.ContentStream,
            result.Value.ContentType,
            fileAsset.OriginalFileName);
    }
}
