using Modules.Finance.Application.DTOs;
using Modules.Finance.Domain.Entities;

namespace Modules.Finance.Application.Mappings;

public static class FinanceMappingExtensions
{
    public static InvoiceItemDto ToDto(this InvoiceItem item, string currency)
    {
        return new InvoiceItemDto(
            item.Id,
            item.InvoiceId,
            item.Description,
            item.Quantity,
            item.UnitPrice.Amount,
            item.Discount.Amount,
            item.TaxRatePercentage,
            item.Tax.Amount,
            item.Total.Amount,
            currency);
    }

    public static PaymentDto ToDto(this Payment payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.InvoiceId,
            payment.Amount.Amount,
            payment.Amount.Currency,
            payment.PaidAt,
            payment.Method,
            payment.Reference,
            payment.Notes);
    }

    public static InvoiceDto ToDto(this Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.CustomerId,
            invoice.ProjectId,
            invoice.OpportunityId,
            invoice.ProposalId,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.Status,
            invoice.Currency,
            invoice.Subtotal.Amount,
            invoice.Discount.Amount,
            invoice.Tax.Amount,
            invoice.Total.Amount,
            invoice.PaidAmount.Amount,
            invoice.OutstandingBalance.Amount,
            invoice.Notes,
            invoice.CreatedAt,
            invoice.UpdatedAt,
            invoice.Items.Select(i => i.ToDto(invoice.Currency)).ToList(),
            invoice.Payments.Select(p => p.ToDto()).ToList());
    }

    public static InvoiceListItemDto ToListItemDto(this Invoice invoice)
    {
        return new InvoiceListItemDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.CustomerId,
            invoice.ProjectId,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.Status,
            invoice.Currency,
            invoice.Total.Amount,
            invoice.PaidAmount.Amount,
            invoice.OutstandingBalance.Amount,
            invoice.CreatedAt);
    }
}
