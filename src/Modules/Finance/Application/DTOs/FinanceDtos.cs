namespace Modules.Finance.Application.DTOs;

public record InvoiceItemDto(
    Guid Id,
    Guid InvoiceId,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal TaxRatePercentage,
    decimal Tax,
    decimal Total,
    string Currency);

public record PaymentDto(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    DateTime PaidAt,
    string Method,
    string Reference,
    string Notes);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid CustomerId,
    Guid? ProjectId,
    Guid? OpportunityId,
    Guid? ProposalId,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Status,
    string Currency,
    decimal Subtotal,
    decimal Discount,
    decimal Tax,
    decimal Total,
    decimal PaidAmount,
    decimal OutstandingBalance,
    string Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<InvoiceItemDto> Items,
    List<PaymentDto> Payments);

public record InvoiceListItemDto(
    Guid Id,
    string InvoiceNumber,
    Guid CustomerId,
    Guid? ProjectId,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Status,
    string Currency,
    decimal Total,
    decimal PaidAmount,
    decimal OutstandingBalance,
    DateTime CreatedAt);

public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public record CreateInvoiceItemRequest(
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount = 0m,
    decimal TaxRatePercentage = 0m);

public record CreateInvoiceRequest(
    Guid CustomerId,
    Guid? ProjectId,
    Guid? OpportunityId,
    Guid? ProposalId,
    DateOnly? IssueDate,
    DateOnly? DueDate,
    string Currency = "USD",
    string Notes = "",
    List<CreateInvoiceItemRequest>? Items = null);

public record UpdateInvoiceRequest(
    Guid? ProjectId,
    Guid? OpportunityId,
    Guid? ProposalId,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Notes);

public record UpdateInvoiceItemRequest(
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount = 0m,
    decimal TaxRatePercentage = 0m);

public record RecordPaymentRequest(
    decimal Amount,
    string Currency,
    DateTime? PaidAt,
    string Method,
    string Reference = "",
    string Notes = "");

public record CancelInvoiceRequest(string Reason = "");
