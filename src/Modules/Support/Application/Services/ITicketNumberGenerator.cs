namespace Modules.Support.Application.Services;

public interface ITicketNumberGenerator
{
    Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default);
}
