using Microsoft.Extensions.Logging;
using Modules.Identity.Application.Common.Interfaces;

namespace Modules.Identity.Infrastructure.Services;

public class AuditEventLogger : IAuditEventLogger
{
    private readonly ILogger<AuditEventLogger> _logger;

    public AuditEventLogger(ILogger<AuditEventLogger> logger)
    {
        _logger = logger;
    }

    public Task LogAsync(string eventType, Guid? userId, string details, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Identity Audit Event: [{EventType}] User: {UserId} - Details: {Details}", eventType, userId, details);
        return Task.CompletedTask;
    }
}
