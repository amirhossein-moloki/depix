using Microsoft.Extensions.Configuration;
using Modules.Platform.Domain.Services;

namespace Modules.Platform.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _storagePath;

    public LocalFileStorage(IConfiguration configuration)
    {
        var configuredPath = configuration["FileStorage:BasePath"];
        _storagePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "uploads")
            : configuredPath;

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
        var uniqueName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine(DateTime.UtcNow.ToString("yyyyMMdd"), uniqueName);
        var fullPath = Path.Combine(_storagePath, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        return relativePath.Replace('\\', '/');
    }

    public Task<(Stream ContentStream, string ContentType, string FileName)?> GetAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var safeKey = storageKey.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_storagePath, safeKey);

        // Path traversal check
        var fullPathNormalized = Path.GetFullPath(fullPath);
        var basePathNormalized = Path.GetFullPath(_storagePath);
        if (!fullPathNormalized.StartsWith(basePathNormalized, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<(Stream, string, string)?>(null);
        }

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<(Stream, string, string)?>(null);
        }

        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            fileStream.CopyTo(memoryStream);
        }
        memoryStream.Position = 0;

        var fileName = Path.GetFileName(fullPath);
        var contentType = GetContentType(fileName);

        return Task.FromResult<(Stream, string, string)?>((memoryStream, contentType, fileName));
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var safeKey = storageKey.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_storagePath, safeKey);

        // Path traversal check
        var fullPathNormalized = Path.GetFullPath(fullPath);
        var basePathNormalized = Path.GetFullPath(_storagePath);
        if (fullPathNormalized.StartsWith(basePathNormalized, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".json" => "application/json",
            ".doc" or ".docx" => "application/msword",
            ".xls" or ".xlsx" => "application/vnd.ms-excel",
            _ => "application/octet-stream"
        };
    }
}
