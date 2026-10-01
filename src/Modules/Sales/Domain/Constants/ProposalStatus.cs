namespace Modules.Sales.Domain.Constants;

public static class ProposalStatus
{
    public const string Draft = "Draft";
    public const string Sent = "Sent";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Expired = "Expired";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyCollection<string> AllStatuses = new[]
    {
        Draft,
        Sent,
        Accepted,
        Rejected,
        Expired,
        Cancelled
    };

    public static bool IsValid(string status)
    {
        return AllStatuses.Any(s => s.Equals(status, StringComparison.OrdinalIgnoreCase));
    }
}
