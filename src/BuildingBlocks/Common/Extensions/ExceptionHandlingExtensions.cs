using BuildingBlocks.Common.Handlers;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Common.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddUnifiedApiResponseAndExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var traceId = context.HttpContext.TraceIdentifier;
                var errors = context.ModelState
                    .Where(m => m.Value?.Errors.Count > 0)
                    .SelectMany(m => m.Value!.Errors.Select(e => new ApiError(
                        ErrorCode.ValidationError,
                        string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid input value." : e.ErrorMessage,
                        m.Key)))
                    .ToList();

                var response = ApiResponse.FailureResponse("Validation failed", errors, traceId);
                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }

    public static IApplicationBuilder UseUnifiedExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(_ => { });
        return app;
    }
}
