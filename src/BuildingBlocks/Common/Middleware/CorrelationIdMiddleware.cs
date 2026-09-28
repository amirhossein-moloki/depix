using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Common.Middleware;

public class CorrelationIdMiddleware
{
    public const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.TraceIdentifier = correlationId!;
        context.Response.Headers[CorrelationIdHeader] = correlationId!;

        await _next(context);
    }
}
