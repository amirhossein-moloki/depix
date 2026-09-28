using BuildingBlocks.Common.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Common.Extensions;

public static class ObservabilityExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
