using BuildingBlocks.Common.Responses;

namespace BuildingBlocks.Common.Exceptions;

public class EntityNotFoundException : Exception
{
    public string Code { get; }

    public EntityNotFoundException(string message, string code = ErrorCode.NotFound)
        : base(message)
    {
        Code = code;
    }

    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.")
    {
        Code = ErrorCode.NotFound;
    }
}
