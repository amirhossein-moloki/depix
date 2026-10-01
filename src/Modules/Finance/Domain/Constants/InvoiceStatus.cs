namespace Modules.Finance.Domain.Constants;

public static class InvoiceStatus
{
    public const string Draft = "DRAFT";
    public const string Issued = "ISSUED";
    public const string PartiallyPaid = "PARTIALLY_PAID";
    public const string Paid = "PAID";
    public const string Overdue = "OVERDUE";
    public const string Cancelled = "CANCELLED";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        Draft,
        Issued,
        PartiallyPaid,
        Paid,
        Overdue,
        Cancelled
    };
}
