using System.Net;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Common.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        var requestPath = httpContext.Request.Path;
        var requestMethod = httpContext.Request.Method;

        var (statusCode, message, errors) = MapException(exception);

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                traceId,
                requestPath,
                requestMethod);
        }
        else
        {
            _logger.LogWarning(
                "Handled exception [{StatusCode}]. Message: {Message}, TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                statusCode,
                exception.Message,
                traceId,
                requestPath,
                requestMethod);
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = ApiResponse.FailureResponse(message, errors, traceId);

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Message, List<ApiError> Errors) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException valEx => (
                (int)HttpStatusCode.BadRequest,
                valEx.Message,
                valEx.Errors.Select(e => new ApiError(valEx.Code, e.Message, e.Field)).ToList()
            ),

            BusinessRuleException busEx => (
                (int)HttpStatusCode.BadRequest,
                busEx.Message,
                new List<ApiError> { new(busEx.Code, busEx.Message) }
            ),

            EntityNotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                notFoundEx.Message,
                new List<ApiError> { new(notFoundEx.Code, notFoundEx.Message) }
            ),

            UnauthorizedException unauthEx => (
                (int)HttpStatusCode.Unauthorized,
                unauthEx.Message,
                new List<ApiError> { new(unauthEx.Code, unauthEx.Message) }
            ),

            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                "Unauthorized access",
                new List<ApiError> { new(ErrorCode.Unauthorized, "Unauthorized access") }
            ),

            ForbiddenException forbEx => (
                (int)HttpStatusCode.Forbidden,
                forbEx.Message,
                new List<ApiError> { new(forbEx.Code, forbEx.Message) }
            ),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.",
                new List<ApiError> { new(ErrorCode.SystemError, "An unexpected error occurred.") }
            )
        };
    }
}
