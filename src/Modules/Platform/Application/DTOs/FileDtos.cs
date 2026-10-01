using BuildingBlocks.Application.CQRS;

namespace Modules.Platform.Application.DTOs;

public record FileAssetDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string FileName,
    string OriginalFileName,
    string ContentType,
    string Extension,
    long Size,
    string StorageKey,
    string StorageProvider,
    string? Description,
    Guid? UploadedBy,
    DateTime CreatedAt);

public record FileDownloadDto(
    Stream ContentStream,
    string ContentType,
    string FileName);

public record UploadFileRequest(
    string EntityType,
    Guid EntityId,
    string? Description);

public record UploadFileCommand(
    string EntityType,
    Guid EntityId,
    string FileName,
    string OriginalFileName,
    string ContentType,
    Stream ContentStream,
    long Size,
    string? Description,
    Guid? UploadedBy) : ICommand<FileAssetDto>;

public record DeleteFileCommand(
    Guid Id,
    Guid? DeletedBy) : ICommand;

public record GetFileMetadataQuery(
    Guid Id) : IQuery<FileAssetDto>;

public record GetFilesByEntityQuery(
    string EntityType,
    Guid EntityId) : IQuery<List<FileAssetDto>>;

public record DownloadFileQuery(
    Guid Id) : IQuery<FileDownloadDto>;
