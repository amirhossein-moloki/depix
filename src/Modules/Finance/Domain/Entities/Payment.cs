using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.ValueObjects;

namespace Modules.Finance.Domain.Entities;

public class Payment : AuditableEntity
{
    public Guid InvoiceId { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public DateTime PaidAt { get; private set; }
    public string Method { get; private set; } = string.Empty;
    public string Reference { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;

    private Payment() { }

    internal Payment(Guid id, Guid invoiceId, Money amount, DateTime paidAt, string method, string reference = "", string notes = "") : base(id)
    {
        if (amount == null || amount.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));
        if (string.IsNullOrWhiteSpace(method))
            throw new ArgumentException("Payment method cannot be empty.", nameof(method));

        InvoiceId = invoiceId;
        Amount = amount;
        PaidAt = paidAt;
        Method = method;
        Reference = reference ?? string.Empty;
        Notes = notes ?? string.Empty;
    }

    public static Payment Create(Guid invoiceId, Money amount, DateTime paidAt, string method, string reference = "", string notes = "")
    {
        return new Payment(Guid.NewGuid(), invoiceId, amount, paidAt, method, reference, notes);
    }
}
