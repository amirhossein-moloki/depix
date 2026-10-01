namespace Modules.Support.Domain.Constants;

public static class TicketStatus
{
    public const string Open = "OPEN";
    public const string InProgress = "IN_PROGRESS";
    public const string WaitingForCustomer = "WAITING_FOR_CUSTOMER";
    public const string Resolved = "RESOLVED";
    public const string Closed = "CLOSED";
    public const string Cancelled = "CANCELLED";

    public static readonly string[] All = { Open, InProgress, WaitingForCustomer, Resolved, Closed, Cancelled };

    public static bool IsValid(string status) => All.Contains(status, StringComparer.OrdinalIgnoreCase);
}
