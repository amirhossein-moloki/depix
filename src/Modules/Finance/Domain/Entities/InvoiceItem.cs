using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.ValueObjects;

namespace Modules.Finance.Domain.Entities;

public class InvoiceItem : Entity
{
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero();
    public Money Discount { get; private set; } = Money.Zero();
    public decimal TaxRatePercentage { get; private set; }
    public Money Tax { get; private set; } = Money.Zero();
    public Money Total { get; private set; } = Money.Zero();

    private InvoiceItem() { }

    internal InvoiceItem(Guid id, Guid invoiceId, string description, int quantity, Money unitPrice, Money? discount = null, decimal taxRatePercentage = 0m) : base(id)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Item description cannot be empty.", nameof(description));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Discount = discount ?? Money.Zero(unitPrice.Currency);
        TaxRatePercentage = taxRatePercentage;

        CalculateTotals();
    }

    public static InvoiceItem Create(Guid invoiceId, string description, int quantity, Money unitPrice, Money? discount = null, decimal taxRatePercentage = 0m)
    {
        return new InvoiceItem(Guid.NewGuid(), invoiceId, description, quantity, unitPrice, discount, taxRatePercentage);
    }

    public void Update(string description, int quantity, Money unitPrice, Money? discount = null, decimal taxRatePercentage = 0m)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Item description cannot be empty.", nameof(description));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Discount = discount ?? Money.Zero(unitPrice.Currency);
        TaxRatePercentage = taxRatePercentage;

        CalculateTotals();
    }

    private void CalculateTotals()
    {
        var rawSubtotal = UnitPrice * Quantity;
        var subtotalAfterDiscount = rawSubtotal - Discount;
        if (subtotalAfterDiscount.Amount < 0)
        {
            subtotalAfterDiscount = Money.Zero(UnitPrice.Currency);
        }

        var taxAmount = Math.Round(subtotalAfterDiscount.Amount * (TaxRatePercentage / 100m), 2, MidpointRounding.AwayFromZero);
        Tax = Money.Create(taxAmount, UnitPrice.Currency);
        Total = subtotalAfterDiscount + Tax;
    }
}
