using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Events;

namespace Modules.Sales.Domain.Entities;

public class Proposal : AuditableAggregateRoot, ISoftDelete
{
    private readonly List<ProposalItem> _proposalItems = new();

    public Guid OpportunityId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Version { get; private set; } = "1.0";
    public string Description { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public DateOnly ValidUntil { get; private set; }
    public string Status { get; private set; } = ProposalStatus.Draft;

    public Money Subtotal { get; private set; } = Money.Zero();
    public Money Discount { get; private set; } = Money.Zero();
    public Money Total { get; private set; } = Money.Zero();

    public string Currency { get; private set; } = "USD";

    // Legacy compatibility properties
    public decimal Amount => Total?.Amount ?? 0m;
    public decimal DiscountAmount => Discount?.Amount ?? 0m;

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public IReadOnlyCollection<ProposalItem> ProposalItems => _proposalItems.AsReadOnly();

    private Proposal() { }

    public Proposal(
        Guid id,
        Guid opportunityId,
        string title,
        string version,
        string description,
        DateOnly validUntil,
        Guid? customerId = null,
        Guid? companyId = null,
        string? notes = null,
        string currency = "USD",
        string status = ProposalStatus.Draft) : base(id)
    {
        if (opportunityId == Guid.Empty)
        {
            throw new ArgumentException("Opportunity ID is required.", nameof(opportunityId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Proposal title is required.", nameof(title));
        }

        OpportunityId = opportunityId;
        Title = title.Trim();
        Version = string.IsNullOrWhiteSpace(version) ? "1.0" : version.Trim();
        Description = description?.Trim() ?? string.Empty;
        Notes = notes?.Trim() ?? string.Empty;
        ValidUntil = validUntil;
        CustomerId = customerId;
        CompanyId = companyId;
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();

        Status = ProposalStatus.IsValid(status) ? status : ProposalStatus.Draft;

        Subtotal = Money.Zero(Currency);
        Discount = Money.Zero(Currency);
        Total = Money.Zero(Currency);
    }

    public static Proposal Create(
        Guid opportunityId,
        string title,
        DateOnly validUntil,
        string version = "1.0",
        string? description = null,
        Guid? customerId = null,
        Guid? companyId = null,
        string? notes = null,
        string currency = "USD")
    {
        var proposal = new Proposal(
            Guid.NewGuid(),
            opportunityId,
            title,
            version,
            description ?? string.Empty,
            validUntil,
            customerId,
            companyId,
            notes,
            currency,
            ProposalStatus.Draft);

        proposal.AddDomainEvent(new ProposalCreatedEvent(
            proposal.Id,
            proposal.OpportunityId,
            proposal.Title,
            proposal.Total));

        return proposal;
    }

    // Legacy Create overload for backward compatibility
    public static Proposal Create(Guid opportunityId, string version, decimal amount, decimal discount, DateOnly validUntil, string status = "DRAFT")
    {
        var proposal = new Proposal(
            Guid.NewGuid(),
            opportunityId,
            $"Proposal v{version}",
            version,
            string.Empty,
            validUntil,
            null,
            null,
            null,
            "USD",
            status);

        proposal.Total = Money.Create(amount, "USD");
        proposal.Discount = Money.Create(discount, "USD");
        proposal.Subtotal = Money.Create(amount + discount, "USD");

        return proposal;
    }

    public void UpdateDetails(
        string title,
        string description,
        DateOnly validUntil,
        string version,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Proposal title is required.", nameof(title));
        }

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        ValidUntil = validUntil;
        Version = string.IsNullOrWhiteSpace(version) ? Version : version.Trim();
        Notes = notes?.Trim() ?? string.Empty;

        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AddItem(string name, string description, int quantity, Money unitPrice, Money? discount = null)
    {
        if (unitPrice.Currency != Currency)
        {
            throw new InvalidOperationException($"Proposal currency ({Currency}) does not match item currency ({unitPrice.Currency}).");
        }

        var item = ProposalItem.Create(Id, name, description, quantity, unitPrice, discount);
        _proposalItems.Add(item);
        RecalculateTotals();
        UpdateTimestamp(DateTime.UtcNow);
    }

    // Legacy compatibility method
    public void AddItem(ProposalItem item)
    {
        if (item != null)
        {
            _proposalItems.Add(item);
            RecalculateTotals();
            UpdateTimestamp(DateTime.UtcNow);
        }
    }

    public void UpdateItem(Guid itemId, string name, string description, int quantity, Money unitPrice, Money? discount = null)
    {
        var item = _proposalItems.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
        {
            throw new InvalidOperationException($"Proposal item with ID '{itemId}' was not found.");
        }

        if (unitPrice.Currency != Currency)
        {
            throw new InvalidOperationException($"Proposal currency ({Currency}) does not match item currency ({unitPrice.Currency}).");
        }

        item.UpdateDetails(name, description, quantity, unitPrice, discount);
        RecalculateTotals();
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _proposalItems.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _proposalItems.Remove(item);
            RecalculateTotals();
            UpdateTimestamp(DateTime.UtcNow);
        }
    }

    public void ChangeStatus(string newStatus, string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(newStatus))
        {
            throw new ArgumentException("Status is required.", nameof(newStatus));
        }

        var normalizedStatus = newStatus.Trim();
        if (!ProposalStatus.IsValid(normalizedStatus))
        {
            throw new InvalidOperationException($"Invalid proposal status '{newStatus}'.");
        }

        var oldStatus = Status;
        Status = normalizedStatus;
        UpdateTimestamp(DateTime.UtcNow);

        if (normalizedStatus.Equals(ProposalStatus.Accepted, StringComparison.OrdinalIgnoreCase) &&
            !oldStatus.Equals(ProposalStatus.Accepted, StringComparison.OrdinalIgnoreCase))
        {
            AddDomainEvent(new ProposalAcceptedEvent(Id, OpportunityId, DateTime.UtcNow));
        }
        else if (normalizedStatus.Equals(ProposalStatus.Rejected, StringComparison.OrdinalIgnoreCase) &&
                 !oldStatus.Equals(ProposalStatus.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            AddDomainEvent(new ProposalRejectedEvent(Id, OpportunityId, reason, DateTime.UtcNow));
        }
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        Status = ProposalStatus.Cancelled;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        Status = ProposalStatus.Draft;
        UpdateTimestamp(DateTime.UtcNow);
    }

    private void RecalculateTotals()
    {
        if (!_proposalItems.Any())
        {
            Subtotal = Money.Zero(Currency);
            Discount = Money.Zero(Currency);
            Total = Money.Zero(Currency);
            return;
        }

        decimal sumSubtotal = 0m;
        decimal sumDiscount = 0m;

        foreach (var item in _proposalItems)
        {
            sumSubtotal += item.UnitPrice.Amount * item.Quantity;
            sumDiscount += item.Discount.Amount;
        }

        Subtotal = Money.Create(sumSubtotal, Currency);
        Discount = Money.Create(sumDiscount, Currency);
        Total = Subtotal.Subtract(Discount);
    }
}
