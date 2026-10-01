namespace BuildingBlocks.Application.Contracts;

public interface IProjectService
{
    Task<bool> ProjectExistsAsync(Guid projectId, CancellationToken cancellationToken = default);
}
