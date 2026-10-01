namespace Modules.Customer.Application.DTOs;

public record CustomerDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string CustomerNumber { get; init; } = string.Empty;
    public DateOnly CustomerSince { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? PrimaryContactId { get; init; }
    public Guid? AssignedTo { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CustomerListItemDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string CustomerNumber { get; init; } = string.Empty;
    public DateOnly CustomerSince { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? PrimaryContactId { get; init; }
    public Guid? AssignedTo { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record CustomerDetailDto
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string CustomerNumber { get; init; } = string.Empty;
    public DateOnly CustomerSince { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? PrimaryContactId { get; init; }
    public Guid? AssignedTo { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateCustomerRequest
{
    public Guid CompanyId { get; init; }
    public string? CustomerNumber { get; init; }
    public DateOnly? CustomerSince { get; init; }
    public Guid? PrimaryContactId { get; init; }
    public Guid? AssignedTo { get; init; }
    public string? Notes { get; init; }
}

public record UpdateCustomerRequest
{
    public Guid? PrimaryContactId { get; init; }
    public string? Notes { get; init; }
    public string? Status { get; init; }
}

public record ChangeCustomerStatusRequest
{
    public string Status { get; init; } = string.Empty;
}

public record SetPrimaryContactRequest
{
    public Guid? PrimaryContactId { get; init; }
}

public record AssignCustomerRequest
{
    public Guid? AssignedTo { get; init; }
}

public record AssignAccountManagerRequest
{
    public Guid? AccountManagerId { get; init; }
}
