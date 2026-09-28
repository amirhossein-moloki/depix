namespace BuildingBlocks.Common.Responses;

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string? Field { get; set; }
    public string Message { get; set; } = string.Empty;

    public ApiError() { }

    public ApiError(string code, string message, string? field = null)
    {
        Code = code;
        Message = message;
        Field = field;
    }
}
