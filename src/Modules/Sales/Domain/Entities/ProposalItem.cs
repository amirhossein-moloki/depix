using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.ValueObjects;

namespace Modules.Sales.Domain.Entities;

public class ProposalItem : Entity
{
    public Guid ProposalId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; } = 1;
    public Money UnitPrice { get; private set; } = Money.Zero();
    public Money Discount { get; private set; } = Money.Zero();
    public Money Total { get; private set; } = Money.Zero();

    // Legacy compatibility property
    public decimal Price => UnitPrice?.Amount ?? 0m;

    private ProposalItem() { }

    public ProposalItem(
        Guid id,
        Guid proposalId,
        string name,
        string description,
        int quantity,
        Money unitPrice,
        Money? discount = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Proposal item name is required.", nameof(name));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        ProposalId = proposalId;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Quantity = quantity;
        UnitPrice = unitPrice ?? Money.Zero();
        Discount = discount ?? Money.Zero(UnitPrice.Currency);

        CalculateTotal();
    }

    public static ProposalItem Create(
        Guid proposalId,
        string name,
        string description,
        int quantity,
        Money unitPrice,
        Money? discount = null)
    {
        return new ProposalItem(Guid.NewGuid(), proposalId, name, description, quantity, unitPrice, discount);
    }

    // Overload for legacy compatibility or simple decimal creation
    public static ProposalItem Create(Guid proposalId, string name, string description, decimal price)
    {
        return Create(proposalId, name, description, 1, Money.Create(price, "USD"));
    }

    public void UpdateDetails(
        string name,
        string description,
        int quantity,
        Money unitPrice,
        Money? discount = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Proposal item name is required.", nameof(name));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Quantity = quantity;
        UnitPrice = unitPrice ?? Money.Zero();
        Discount = discount ?? Money.Zero(UnitPrice.Currency);

        CalculateTotal();
    }

    private void CalculateTotal()
    {
        var lineSubtotal = Money.Create(UnitPrice.Amount * Quantity, UnitPrice.Currency);
        if (Discount != null && Discount.Amount > 0)
        {
            Total = lineSubtotal.Subtract(Discount);
        }
        else
        {
            Total = lineSubtotal;
        }
    }
}
