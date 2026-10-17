using System.Net;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Handlers;
using BuildingBlocks.Common.Middleware;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests;

public class LoggingAndCorrelationTests
{
    [Fact]
    public async Task CorrelationIdMiddleware_WhenHeaderMissing_GeneratesCorrelationIdAndSetsHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(next: (innerContext) => Task.CompletedTask, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.TryGetValue(CorrelationIdMiddleware.CorrelationIdHeader, out var correlationIdHeader));
        Assert.False(string.IsNullOrWhiteSpace(correlationIdHeader));
        Guid.Parse(correlationIdHeader!);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_WhenValidHeaderProvided_PreservesCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var customCorrelationId = "custom-corr-12345";
        context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeader] = customCorrelationId;

        var loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(next: (innerContext) => Task.CompletedTask, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(customCorrelationId, context.Response.Headers[CorrelationIdMiddleware.CorrelationIdHeader].ToString());
    }

    [Fact]
    public async Task CorrelationIdMiddleware_WhenExcessivelyLongHeaderProvided_ReplacesWithNewCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var invalidLongHeader = new string('A', 200);
        context.Request.Headers[CorrelationIdMiddleware.CorrelationIdHeader] = invalidLongHeader;

        var loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        var middleware = new CorrelationIdMiddleware(next: (innerContext) => Task.CompletedTask, loggerMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseHeader = context.Response.Headers[CorrelationIdMiddleware.CorrelationIdHeader].ToString();
        Assert.NotEqual(invalidLongHeader, responseHeader);
        Assert.True(Guid.TryParse(responseHeader, out _));
    }

    [Fact]
    public async Task GlobalExceptionHandler_HandledValidationException_ReturnsBadRequestWithTraceId()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(loggerMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.TraceIdentifier = "trace-id-test-123";

        var valEx = new ValidationException("Validation failed.", new List<ValidationErrorDetail>
        {
            new("Email", "Email is required")
        });

        // Act
        var handled = await handler.TryHandleAsync(httpContext, valEx, CancellationToken.None);

        // Assert
        Assert.True(handled);
        Assert.Equal((int)HttpStatusCode.BadRequest, httpContext.Response.StatusCode);
    }
}
