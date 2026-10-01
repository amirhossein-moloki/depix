using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Finance.Domain.Constants;
using Modules.Finance.Domain.Entities;
using Modules.Finance.Domain.Events;
using Xunit;

namespace UnitTests;

public class InvoiceDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializeInvoiceInDraftStatus()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var invoiceNumber = "INV-202610-0001";

        // Act
        var invoice = Invoice.Create(invoiceNumber, customerId);

        // Assert
        Assert.NotNull(invoice);
        Assert.Equal(invoiceNumber, invoice.InvoiceNumber);
        Assert.Equal(customerId, invoice.CustomerId);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Equal("USD", invoice.Currency);
        Assert.Equal(0m, invoice.Subtotal.Amount);
        Assert.Equal(0m, invoice.Discount.Amount);
        Assert.Equal(0m, invoice.Tax.Amount);
        Assert.Equal(0m, invoice.Total.Amount);
        Assert.Equal(0m, invoice.PaidAmount.Amount);
        Assert.Equal(0m, invoice.OutstandingBalance.Amount);
        Assert.Empty(invoice.Items);
        Assert.Empty(invoice.Payments);
        Assert.Contains(invoice.DomainEvents, e => e is InvoiceCreatedEvent);
    }

    [Fact]
    public void Create_WithoutCustomerId_ShouldThrowBusinessRuleException()
    {
        Assert.Throws<BusinessRuleException>(() => Invoice.Create("INV-001", Guid.Empty));
    }

    [Fact]
    public void AddItem_ShouldCalculateTotalsAndOutstandingBalanceCorrectly()
    {
        // Arrange
        var invoice = Invoice.Create("INV-001", Guid.NewGuid(), currency: "USD");

        // Act
        invoice.AddItem("Software Consulting", 10, Money.Create(100m, "USD"), Money.Create(50m, "USD"), 10m);

        // Assert
        // Line subtotal = 10 * 100 = 1000
        // Line discount = 50
        // Net subtotal = 950
        // Tax (10%) = 95
        // Line Total = 1045
        Assert.Single(invoice.Items);
        Assert.Equal(1000m, invoice.Subtotal.Amount);
        Assert.Equal(50m, invoice.Discount.Amount);
        Assert.Equal(95m, invoice.Tax.Amount);
        Assert.Equal(1045m, invoice.Total.Amount);
        Assert.Equal(1045m, invoice.OutstandingBalance.Amount);
    }

    [Fact]
    public void AddItem_WithMismatchedCurrency_ShouldThrowBusinessRuleException()
    {
        var invoice = Invoice.Create("INV-001", Guid.NewGuid(), currency: "USD");

        Assert.Throws<BusinessRuleException>(() =>
            invoice.AddItem("Item", 1, Money.Create(100m, "EUR")));
    }

    [Fact]
    public void Issue_WithValidItems_ShouldTransitionStatusToIssued()
    {
        // Arrange
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(500m, "USD"));

        // Act
        invoice.Issue();

        // Assert
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Contains(invoice.DomainEvents, e => e is InvoiceIssuedEvent);
    }

    [Fact]
    public void Issue_WithoutItems_ShouldThrowBusinessRuleException()
    {
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());

        Assert.Throws<BusinessRuleException>(() => invoice.Issue());
    }

    [Fact]
    public void AddItem_OnIssuedInvoice_ShouldThrowBusinessRuleException()
    {
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(500m, "USD"));
        invoice.Issue();

        Assert.Throws<BusinessRuleException>(() =>
            invoice.AddItem("New Service", 1, Money.Create(100m, "USD")));
    }

    [Fact]
    public void RecordPayment_PartialAmount_ShouldUpdateStatusToPartiallyPaidAndReduceOutstandingBalance()
    {
        // Arrange
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(1000m, "USD"));
        invoice.Issue();

        // Act
        var payment = invoice.RecordPayment(Money.Create(400m, "USD"), DateTime.UtcNow, PaymentMethod.BankTransfer, "REF123", "First installment");

        // Assert
        Assert.NotNull(payment);
        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
        Assert.Equal(400m, invoice.PaidAmount.Amount);
        Assert.Equal(600m, invoice.OutstandingBalance.Amount);
        Assert.Contains(invoice.DomainEvents, e => e is PaymentRecordedEvent);
    }

    [Fact]
    public void RecordPayment_FullRemainingAmount_ShouldUpdateStatusToPaid()
    {
        // Arrange
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(1000m, "USD"));
        invoice.Issue();
        invoice.RecordPayment(Money.Create(400m, "USD"), DateTime.UtcNow, PaymentMethod.BankTransfer);

        // Act
        invoice.RecordPayment(Money.Create(600m, "USD"), DateTime.UtcNow, PaymentMethod.Card);

        // Assert
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(1000m, invoice.PaidAmount.Amount);
        Assert.Equal(0m, invoice.OutstandingBalance.Amount);
        Assert.Contains(invoice.DomainEvents, e => e is InvoicePaidEvent);
    }

    [Fact]
    public void RecordPayment_ExceedingOutstandingBalance_ShouldThrowBusinessRuleException()
    {
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(500m, "USD"));
        invoice.Issue();

        Assert.Throws<BusinessRuleException>(() =>
            invoice.RecordPayment(Money.Create(600m, "USD"), DateTime.UtcNow, PaymentMethod.Cash));
    }

    [Fact]
    public void RecordPayment_OnDraftInvoice_ShouldThrowBusinessRuleException()
    {
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(500m, "USD"));

        Assert.Throws<BusinessRuleException>(() =>
            invoice.RecordPayment(Money.Create(500m, "USD"), DateTime.UtcNow, PaymentMethod.Cash));
    }

    [Fact]
    public void Cancel_UnpaidIssuedInvoice_ShouldTransitionStatusToCancelled()
    {
        // Arrange
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(500m, "USD"));
        invoice.Issue();

        // Act
        invoice.Cancel("Customer project terminated");

        // Assert
        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        Assert.Contains(invoice.DomainEvents, e => e is InvoiceCancelledEvent);
    }

    [Fact]
    public void RecordPayment_OnCancelledInvoice_ShouldThrowBusinessRuleException()
    {
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(500m, "USD"));
        invoice.Issue();
        invoice.Cancel("Terminated");

        Assert.Throws<BusinessRuleException>(() =>
            invoice.RecordPayment(Money.Create(500m, "USD"), DateTime.UtcNow, PaymentMethod.Cash));
    }

    [Fact]
    public void CheckAndMarkOverdue_WhenPastDueDateWithOutstandingBalance_ShouldMarkAsOverdue()
    {
        // Arrange
        var issueDate = new DateOnly(2026, 1, 1);
        var dueDate = new DateOnly(2026, 1, 15);
        var invoice = Invoice.Create("INV-001", Guid.NewGuid(), issueDate: issueDate, dueDate: dueDate);
        invoice.AddItem("Service", 1, Money.Create(500m, "USD"));
        invoice.Issue();

        // Act
        invoice.CheckAndMarkOverdue(new DateOnly(2026, 1, 20));

        // Assert
        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
    }
}
