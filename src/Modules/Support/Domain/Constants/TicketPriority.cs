namespace Modules.Support.Domain.Constants;

public static class TicketPriority
{
    public const string Low = "LOW";
    public const string Normal = "NORMAL";
    public const string High = "HIGH";
    public const string Urgent = "URGENT";

    public static readonly string[] All = { Low, Normal, High, Urgent };

    public static bool IsValid(string priority) => All.Contains(priority, StringComparer.OrdinalIgnoreCase);
}
