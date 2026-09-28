using BuildingBlocks.Common.Responses;

namespace BuildingBlocks.Common.Exceptions;

public class ForbiddenException : Exception
{
    public string Code { get; }

    public ForbiddenException(string message = "Forbidden access", string code = ErrorCode.Forbidden)
        : base(message)
    {
        Code = code;
    }
}
