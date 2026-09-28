namespace BuildingBlocks.Common.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ApiError> Errors { get; set; } = new();
    public string TraceId { get; set; } = string.Empty;

    public ApiResponse() { }

    public ApiResponse(bool success, string message, T? data, List<ApiError>? errors = null, string traceId = "")
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors ?? new List<ApiError>();
        TraceId = traceId;
    }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully", string traceId = "")
    {
        return new ApiResponse<T>(true, message, data, new List<ApiError>(), traceId);
    }

    public static ApiResponse<T> FailureResponse(string message, List<ApiError>? errors = null, string traceId = "")
    {
        return new ApiResponse<T>(false, message, default, errors ?? new List<ApiError>(), traceId);
    }

    public static ApiResponse<T> FailureResponse(string message, ApiError error, string traceId = "")
    {
        return new ApiResponse<T>(false, message, default, new List<ApiError> { error }, traceId);
    }
}

public class ApiResponse : ApiResponse<object>
{
    public ApiResponse() { }

    public ApiResponse(bool success, string message, object? data, List<ApiError>? errors = null, string traceId = "")
        : base(success, message, data, errors, traceId)
    {
    }

    public static ApiResponse SuccessResponse(string message = "Operation completed successfully", string traceId = "")
    {
        return new ApiResponse(true, message, null, new List<ApiError>(), traceId);
    }

    public static new ApiResponse FailureResponse(string message, List<ApiError>? errors = null, string traceId = "")
    {
        return new ApiResponse(false, message, null, errors ?? new List<ApiError>(), traceId);
    }

    public static new ApiResponse FailureResponse(string message, ApiError error, string traceId = "")
    {
        return new ApiResponse(false, message, null, new List<ApiError> { error }, traceId);
    }
}
