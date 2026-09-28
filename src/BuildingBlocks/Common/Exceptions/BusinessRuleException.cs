using BuildingBlocks.Common.Responses;

namespace BuildingBlocks.Common.Exceptions;

public class BusinessRuleException : Exception
{
    public string Code { get; }

    public BusinessRuleException(string message, string code = ErrorCode.BusinessRuleViolation)
        : base(message)
    {
        Code = code;
    }

    public BusinessRuleException(string message, Exception innerException, string code = ErrorCode.BusinessRuleViolation)
        : base(message, innerException)
    {
        Code = code;
    }
}
