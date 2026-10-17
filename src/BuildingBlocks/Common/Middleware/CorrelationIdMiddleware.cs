using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Common.Middleware;

public class CorrelationIdMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) ||
            string.IsNullOrWhiteSpace(correlationId) ||
            correlationId.ToString().Length > 100)
        {
            correlationId = Guid.NewGuid().ToString();
        }

        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        if (string.IsNullOrWhiteSpace(traceId))
        {
            traceId = correlationId!;
        }

        context.TraceIdentifier = traceId;
        context.Response.Headers[CorrelationIdHeader] = correlationId!;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId.ToString(),
            ["TraceId"] = traceId
        }))
        {
            await _next(context);
        }
    }
}
