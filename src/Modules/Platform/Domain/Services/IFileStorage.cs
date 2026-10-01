namespace Modules.Platform.Domain.Services;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<(Stream ContentStream, string ContentType, string FileName)?> GetAsync(string storageKey, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}
