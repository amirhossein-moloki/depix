using System.Text.Json;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Handlers;
using BuildingBlocks.Common.Responses;
using BuildingBlocks.SharedKernel.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CrmErp.ArchitectureTests;

public class ResponseAndExceptionHandlingTests
{
    [Fact]
    public void SuccessResponse_CreatesCorrectStructure()
    {
        var data = new { Id = 1, Name = "Test" };
        var traceId = "test-trace-123";

        var response = ApiResponse<object>.SuccessResponse(data, "Success message", traceId);

        Assert.True(response.Success);
        Assert.Equal("Success message", response.Message);
        Assert.Equal(data, response.Data);
        Assert.Empty(response.Errors);
        Assert.Equal(traceId, response.TraceId);
    }

    [Fact]
    public void FailureResponse_CreatesCorrectErrorStructure()
    {
        var errors = new List<ApiError>
        {
            new(ErrorCode.ValidationError, "Invalid email", "Email")
        };
        var traceId = "test-trace-456";

        var response = ApiResponse<object>.FailureResponse("Validation failed", errors, traceId);

        Assert.False(response.Success);
        Assert.Equal("Validation failed", response.Message);
        Assert.Null(response.Data);
        Assert.Single(response.Errors);
        Assert.Equal(ErrorCode.ValidationError, response.Errors[0].Code);
        Assert.Equal("Email", response.Errors[0].Field);
        Assert.Equal("Invalid email", response.Errors[0].Message);
        Assert.Equal(traceId, response.TraceId);
    }

    [Fact]
    public void Result_ToApiResponse_ConvertsSuccessAndFailure()
    {
        var successResult = Result.Success("Sample Payload");
        var successResponse = successResult.ToApiResponse("trace-1");

        Assert.True(successResponse.Success);
        Assert.Equal("Sample Payload", successResponse.Data);

        var failError = new Error("CUSTOM_ERR", "Something went wrong");
        var failResult = Result.Failure<string>(failError);
        var failResponse = failResult.ToApiResponse("trace-2");

        Assert.False(failResponse.Success);
        Assert.Single(failResponse.Errors);
        Assert.Equal("CUSTOM_ERR", failResponse.Errors[0].Code);
        Assert.Equal("Something went wrong", failResponse.Errors[0].Message);
    }

    [Theory]
    [InlineData(typeof(BusinessRuleException), 400)]
    [InlineData(typeof(ValidationException), 400)]
    [InlineData(typeof(EntityNotFoundException), 404)]
    [InlineData(typeof(UnauthorizedException), 401)]
    [InlineData(typeof(ForbiddenException), 403)]
    [InlineData(typeof(InvalidOperationException), 500)]
    public async Task GlobalExceptionHandler_MapsExceptionsToCorrectStatusCodes(Type exceptionType, int expectedStatusCode)
    {
        var logger = NullLogger<GlobalExceptionHandler>.Instance;
        var handler = new GlobalExceptionHandler(logger);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "test-trace-id";

        Exception ex = exceptionType switch
        {
            var t when t == typeof(BusinessRuleException) => new BusinessRuleException("Business rule violated"),
            var t when t == typeof(ValidationException) => new ValidationException("field1", "Field 1 error"),
            var t when t == typeof(EntityNotFoundException) => new EntityNotFoundException("Customer", 123),
            var t when t == typeof(UnauthorizedException) => new UnauthorizedException("Not logged in"),
            var t when t == typeof(ForbiddenException) => new ForbiddenException("Access denied"),
            _ => new InvalidOperationException("Internal server error details that should not be exposed")
        };

        var handled = await handler.TryHandleAsync(context, ex, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedStatusCode, context.Response.StatusCode);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var responseJson = await reader.ReadToEndAsync();

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        Assert.False(root.GetProperty("success").GetBoolean());
        Assert.Equal("test-trace-id", root.GetProperty("traceId").GetString());

        if (expectedStatusCode == 500)
        {
            Assert.DoesNotContain("Internal server error details that should not be exposed", responseJson);
            Assert.Equal("An unexpected error occurred. Please try again later.", root.GetProperty("message").GetString());
        }
    }
}
