namespace BuildingBlocks.Common.Responses;

public static class ResponseFactory
{
    public static ApiResponse<T> Success<T>(T data, string message = "Operation completed successfully", string traceId = "")
    {
        return ApiResponse<T>.SuccessResponse(data, message, traceId);
    }

    public static ApiResponse Success(string message = "Operation completed successfully", string traceId = "")
    {
        return ApiResponse.SuccessResponse(message, traceId);
    }

    public static ApiResponse<T> Failure<T>(string message, List<ApiError>? errors = null, string traceId = "")
    {
        return ApiResponse<T>.FailureResponse(message, errors, traceId);
    }

    public static ApiResponse Failure(string message, List<ApiError>? errors = null, string traceId = "")
    {
        return ApiResponse.FailureResponse(message, errors, traceId);
    }

    public static ApiResponse Failure(string message, ApiError error, string traceId = "")
    {
        return ApiResponse.FailureResponse(message, error, traceId);
    }
}
