using BuildingBlocks.SharedKernel.Results;

namespace BuildingBlocks.Common.Responses;

public static class ResponseExtensions
{
    public static ApiResponse<T> ToApiResponse<T>(this T data, string message = "Operation completed successfully", string traceId = "")
    {
        return ResponseFactory.Success(data, message, traceId);
    }

    public static ApiResponse<T> ToApiResponse<T>(this Result<T> result, string traceId = "")
    {
        if (result.IsSuccess)
        {
            return ResponseFactory.Success(result.Value, traceId: traceId);
        }

        var apiError = new ApiError(
            string.IsNullOrEmpty(result.Error.Code) ? ErrorCode.BusinessRuleViolation : result.Error.Code,
            result.Error.Description);

        return ResponseFactory.Failure<T>(result.Error.Description, new List<ApiError> { apiError }, traceId);
    }

    public static ApiResponse ToApiResponse(this Result result, string traceId = "")
    {
        if (result.IsSuccess)
        {
            return ResponseFactory.Success(traceId: traceId);
        }

        var apiError = new ApiError(
            string.IsNullOrEmpty(result.Error.Code) ? ErrorCode.BusinessRuleViolation : result.Error.Code,
            result.Error.Description);

        return ResponseFactory.Failure(result.Error.Description, new List<ApiError> { apiError }, traceId);
    }
}
