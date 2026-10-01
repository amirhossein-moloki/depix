namespace Modules.Support.Domain.Constants;

public static class TicketCategory
{
    public const string Bug = "BUG";
    public const string TechnicalIssue = "TECHNICAL_ISSUE";
    public const string ContentChange = "CONTENT_CHANGE";
    public const string Deployment = "DEPLOYMENT";
    public const string Domain = "DOMAIN";
    public const string General = "GENERAL";
    public const string Other = "OTHER";

    public static readonly string[] All = { Bug, TechnicalIssue, ContentChange, Deployment, Domain, General, Other };

    public static bool IsValid(string category) => All.Contains(category, StringComparer.OrdinalIgnoreCase);
}
