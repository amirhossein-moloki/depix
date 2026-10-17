using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Platform.Application.DTOs;

namespace Modules.Platform.API.Controllers;

[ApiController]
[Route("api/platform/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly ICommandHandler<UploadFileCommand, FileAssetDto> _uploadFileHandler;
    private readonly ICommandHandler<DeleteFileCommand> _deleteFileHandler;
    private readonly IQueryHandler<GetFileMetadataQuery, FileAssetDto> _getFileMetadataHandler;
    private readonly IQueryHandler<GetFilesByEntityQuery, List<FileAssetDto>> _getFilesByEntityHandler;
    private readonly IQueryHandler<DownloadFileQuery, FileDownloadDto> _downloadFileHandler;
    private readonly IValidator<UploadFileCommand> _uploadValidator;

    public FilesController(
        ICommandHandler<UploadFileCommand, FileAssetDto> uploadFileHandler,
        ICommandHandler<DeleteFileCommand> deleteFileHandler,
        IQueryHandler<GetFileMetadataQuery, FileAssetDto> getFileMetadataHandler,
        IQueryHandler<GetFilesByEntityQuery, List<FileAssetDto>> getFilesByEntityHandler,
        IQueryHandler<DownloadFileQuery, FileDownloadDto> downloadFileHandler,
        IValidator<UploadFileCommand> uploadValidator)
    {
        _uploadFileHandler = uploadFileHandler;
        _deleteFileHandler = deleteFileHandler;
        _getFileMetadataHandler = getFileMetadataHandler;
        _getFilesByEntityHandler = getFilesByEntityHandler;
        _downloadFileHandler = downloadFileHandler;
        _uploadValidator = uploadValidator;
    }

    [HttpPost]
    [Authorize(Policy = "Platform.File.Create")]
    public async Task<IActionResult> UploadFile(
        [FromForm] IFormFile file,
        [FromForm] string entityType,
        [FromForm] Guid entityId,
        [FromForm] string? description,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            throw new BuildingBlocks.Common.Exceptions.ValidationException("File upload failed.", new List<ValidationErrorDetail>
            {
                new("file", "No file was provided.")
            });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? uploadedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        using var stream = file.OpenReadStream();
        var command = new UploadFileCommand(
            entityType,
            entityId,
            file.FileName,
            file.FileName,
            file.ContentType,
            stream,
            file.Length,
            description,
            uploadedBy);

        var validationResult = await _uploadValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("File upload validation failed.", errors);
        }

        var result = await _uploadFileHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetFileMetadata), new { id = result.Id }, ResponseFactory.Success(result, "File uploaded successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Platform.File.Read")]
    public async Task<IActionResult> GetFileMetadata(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetFileMetadataQuery(id);
        var fileAsset = await _getFileMetadataHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(fileAsset, "File metadata retrieved successfully."));
    }

    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = "Platform.File.Read")]
    public async Task<IActionResult> DownloadFile(Guid id, CancellationToken cancellationToken)
    {
        var query = new DownloadFileQuery(id);
        var downloadDto = await _downloadFileHandler.HandleAsync(query, cancellationToken);
        return File(downloadDto.ContentStream, downloadDto.ContentType, downloadDto.FileName);
    }

    [HttpGet]
    [Authorize(Policy = "Platform.File.Read")]
    public async Task<IActionResult> GetFilesByEntity(
        [FromQuery] string entityType,
        [FromQuery] Guid entityId,
        CancellationToken cancellationToken)
    {
        var query = new GetFilesByEntityQuery(entityType, entityId);
        var files = await _getFilesByEntityHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(files, "Entity files retrieved successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Platform.File.Delete")]
    public async Task<IActionResult> DeleteFile(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? deletedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new DeleteFileCommand(id, deletedBy);
        await _deleteFileHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("File deleted successfully.", string.Empty));
    }
}
