using BuildingBlocks.Common.Responses;

namespace BuildingBlocks.Common.Exceptions;

public class ValidationErrorDetail
{
    public string Field { get; }
    public string Message { get; }

    public ValidationErrorDetail(string field, string message)
    {
        Field = field;
        Message = message;
    }
}

public class ValidationException : Exception
{
    public string Code { get; }
    public IReadOnlyCollection<ValidationErrorDetail> Errors { get; }

    public ValidationException(string message, IEnumerable<ValidationErrorDetail>? errors = null, string code = ErrorCode.ValidationError)
        : base(message)
    {
        Code = code;
        Errors = errors?.ToList().AsReadOnly() ?? new List<ValidationErrorDetail>().AsReadOnly();
    }

    public ValidationException(IEnumerable<ValidationErrorDetail> errors)
        : this("Validation failed", errors)
    {
    }

    public ValidationException(string field, string message)
        : this("Validation failed", new[] { new ValidationErrorDetail(field, message) })
    {
    }
}
