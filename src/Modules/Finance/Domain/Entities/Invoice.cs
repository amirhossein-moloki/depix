using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Finance.Domain.Constants;
using Modules.Finance.Domain.Events;

namespace Modules.Finance.Domain.Entities;

public class Invoice : AuditableAggregateRoot, ISoftDelete
{
    private readonly List<InvoiceItem> _items = new();
    private readonly List<Payment> _payments = new();

    public string InvoiceNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public Guid? OpportunityId { get; private set; }
    public Guid? ProposalId { get; private set; }

    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string Status { get; private set; } = InvoiceStatus.Draft;
    public string Currency { get; private set; } = "USD";

    public Money Subtotal { get; private set; } = Money.Zero();
    public Money Discount { get; private set; } = Money.Zero();
    public Money Tax { get; private set; } = Money.Zero();
    public Money Total { get; private set; } = Money.Zero();
    public Money PaidAmount { get; private set; } = Money.Zero();
    public Money OutstandingBalance { get; private set; } = Money.Zero();

    public string Notes { get; private set; } = string.Empty;

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private Invoice() { }

    private Invoice(
        Guid id,
        string invoiceNumber,
        Guid customerId,
        Guid? projectId,
        Guid? opportunityId,
        Guid? proposalId,
        DateOnly issueDate,
        DateOnly dueDate,
        string currency,
        string notes) : base(id)
    {
        if (customerId == Guid.Empty)
            throw new BusinessRuleException("Invoice requires a valid CustomerId.");
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new BusinessRuleException("Invoice number cannot be empty.");
        if (dueDate < issueDate)
            throw new BusinessRuleException("DueDate cannot be earlier than IssueDate.");

        InvoiceNumber = invoiceNumber;
        CustomerId = customerId;
        ProjectId = projectId;
        OpportunityId = opportunityId;
        ProposalId = proposalId;
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.ToUpperInvariant();
        Notes = notes ?? string.Empty;
        Status = InvoiceStatus.Draft;

        Subtotal = Money.Zero(Currency);
        Discount = Money.Zero(Currency);
        Tax = Money.Zero(Currency);
        Total = Money.Zero(Currency);
        PaidAmount = Money.Zero(Currency);
        OutstandingBalance = Money.Zero(Currency);

        AddDomainEvent(new InvoiceCreatedEvent(Id, CustomerId, InvoiceNumber));
    }

    public static Invoice Create(
        string invoiceNumber,
        Guid customerId,
        Guid? projectId = null,
        Guid? opportunityId = null,
        Guid? proposalId = null,
        DateOnly? issueDate = null,
        DateOnly? dueDate = null,
        string currency = "USD",
        string notes = "")
    {
        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var actualIssueDate = issueDate ?? now;
        var actualDueDate = dueDate ?? actualIssueDate.AddDays(30);

        return new Invoice(
            Guid.NewGuid(),
            invoiceNumber,
            customerId,
            projectId,
            opportunityId,
            proposalId,
            actualIssueDate,
            actualDueDate,
            currency,
            notes);
    }

    public void UpdateHeader(
        Guid? projectId,
        Guid? opportunityId,
        Guid? proposalId,
        DateOnly issueDate,
        DateOnly dueDate,
        string notes)
    {
        EnsureModifiable();

        if (dueDate < issueDate)
            throw new BusinessRuleException("DueDate cannot be earlier than IssueDate.");

        ProjectId = projectId;
        OpportunityId = opportunityId;
        ProposalId = proposalId;
        IssueDate = issueDate;
        DueDate = dueDate;
        Notes = notes ?? string.Empty;

        UpdateTimestamp(DateTime.UtcNow);
    }

    public InvoiceItem AddItem(
        string description,
        int quantity,
        Money unitPrice,
        Money? discount = null,
        decimal taxRatePercentage = 0m)
    {
        EnsureModifiable();

        if (unitPrice.Currency != Currency)
            throw new BusinessRuleException($"Item currency ({unitPrice.Currency}) must match invoice currency ({Currency}).");

        var item = InvoiceItem.Create(Id, description, quantity, unitPrice, discount, taxRatePercentage);
        _items.Add(item);

        RecalculateTotals();
        UpdateTimestamp(DateTime.UtcNow);

        return item;
    }

    public void UpdateItem(
        Guid itemId,
        string description,
        int quantity,
        Money unitPrice,
        Money? discount = null,
        decimal taxRatePercentage = 0m)
    {
        EnsureModifiable();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new EntityNotFoundException("InvoiceItem", itemId);

        if (unitPrice.Currency != Currency)
            throw new BusinessRuleException($"Item currency ({unitPrice.Currency}) must match invoice currency ({Currency}).");

        item.Update(description, quantity, unitPrice, discount, taxRatePercentage);

        RecalculateTotals();
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureModifiable();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new EntityNotFoundException("InvoiceItem", itemId);

        _items.Remove(item);

        RecalculateTotals();
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void Issue()
    {
        if (Status != InvoiceStatus.Draft)
            throw new BusinessRuleException($"Only draft invoices can be issued. Current status: {Status}");

        if (!_items.Any())
            throw new BusinessRuleException("Invoice cannot be issued without valid items.");

        if (Total.Amount <= 0)
            throw new BusinessRuleException("Invoice total must be greater than zero to be issued.");

        Status = InvoiceStatus.Issued;
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new InvoiceIssuedEvent(Id, CustomerId, InvoiceNumber, Total.Amount, Currency, DueDate));
    }

    public Payment RecordPayment(
        Money amount,
        DateTime paidAt,
        string method,
        string reference = "",
        string notes = "")
    {
        if (Status == InvoiceStatus.Cancelled)
            throw new BusinessRuleException("Cannot record payment for a cancelled invoice.");

        if (Status == InvoiceStatus.Draft)
            throw new BusinessRuleException("Cannot record payment for a draft invoice. Issue the invoice first.");

        if (amount == null || amount.Amount <= 0)
            throw new BusinessRuleException("Payment amount must be greater than zero.");

        if (amount.Currency != Currency)
            throw new BusinessRuleException($"Payment currency ({amount.Currency}) does not match invoice currency ({Currency}).");

        if (amount.Amount > OutstandingBalance.Amount)
            throw new BusinessRuleException($"Payment amount ({amount.Amount}) exceeds outstanding balance ({OutstandingBalance.Amount}).");

        var payment = Payment.Create(Id, amount, paidAt, method, reference, notes);
        _payments.Add(payment);

        PaidAmount += amount;
        OutstandingBalance -= amount;

        if (OutstandingBalance.Amount == 0)
        {
            Status = InvoiceStatus.Paid;
            AddDomainEvent(new InvoicePaidEvent(Id, CustomerId, InvoiceNumber, Total.Amount));
        }
        else
        {
            Status = InvoiceStatus.PartiallyPaid;
        }

        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new PaymentRecordedEvent(payment.Id, Id, CustomerId, amount.Amount, Currency, method));

        return payment;
    }

    public void Cancel(string reason = "")
    {
        if (Status == InvoiceStatus.Paid)
            throw new BusinessRuleException("Fully paid invoices cannot be cancelled.");

        if (Status == InvoiceStatus.Cancelled)
            throw new BusinessRuleException("Invoice is already cancelled.");

        Status = InvoiceStatus.Cancelled;
        UpdateTimestamp(DateTime.UtcNow);

        AddDomainEvent(new InvoiceCancelledEvent(Id, reason));
    }

    public void CheckAndMarkOverdue(DateOnly currentDate)
    {
        if ((Status == InvoiceStatus.Issued || Status == InvoiceStatus.PartiallyPaid) && currentDate > DueDate && OutstandingBalance.Amount > 0)
        {
            Status = InvoiceStatus.Overdue;
            UpdateTimestamp(DateTime.UtcNow);
        }
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }

    private void EnsureModifiable()
    {
        if (Status != InvoiceStatus.Draft)
            throw new BusinessRuleException($"Invoice cannot be modified when status is '{Status}'. Only draft invoices are modifiable.");
    }

    private void RecalculateTotals()
    {
        decimal totalSubtotal = 0m;
        decimal totalDiscount = 0m;
        decimal totalTax = 0m;

        foreach (var item in _items)
        {
            totalSubtotal += item.UnitPrice.Amount * item.Quantity;
            totalDiscount += item.Discount.Amount;
            totalTax += item.Tax.Amount;
        }

        Subtotal = Money.Create(totalSubtotal, Currency);
        Discount = Money.Create(totalDiscount, Currency);
        Tax = Money.Create(totalTax, Currency);
        Total = Money.Create(totalSubtotal - totalDiscount + totalTax, Currency);

        OutstandingBalance = Money.Create(Total.Amount - PaidAmount.Amount, Currency);
    }
}
