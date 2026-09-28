using BuildingBlocks.Common.Responses;

namespace BuildingBlocks.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public string Code { get; }

    public UnauthorizedException(string message = "Unauthorized access", string code = ErrorCode.Unauthorized)
        : base(message)
    {
        Code = code;
    }
}
