using BuildingBlocks.Domain.Events;

namespace Modules.Finance.Domain.Events;

public record InvoiceCreatedEvent(Guid InvoiceId, Guid CustomerId, string InvoiceNumber) : DomainEvent;

public record InvoiceIssuedEvent(Guid InvoiceId, Guid CustomerId, string InvoiceNumber, decimal TotalAmount, string Currency, DateOnly DueDate) : DomainEvent;

public record InvoiceCancelledEvent(Guid InvoiceId, string Reason) : DomainEvent;

public record PaymentRecordedEvent(Guid PaymentId, Guid InvoiceId, Guid CustomerId, decimal Amount, string Currency, string Method) : DomainEvent;

public record InvoicePaidEvent(Guid InvoiceId, Guid CustomerId, string InvoiceNumber, decimal TotalAmount) : DomainEvent;
