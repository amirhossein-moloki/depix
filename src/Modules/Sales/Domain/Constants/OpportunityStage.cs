namespace Modules.Sales.Domain.Constants;

public static class OpportunityStage
{
    public const string New = "New";
    public const string Discovery = "Discovery";
    public const string Qualified = "Qualified";
    public const string Proposal = "Proposal";
    public const string Negotiation = "Negotiation";
    public const string Won = "Won";
    public const string Lost = "Lost";

    public static readonly IReadOnlyCollection<string> AllStages = new[]
    {
        New,
        Discovery,
        Qualified,
        Proposal,
        Negotiation,
        Won,
        Lost
    };

    public static bool IsValid(string stage)
    {
        return AllStages.Any(s => s.Equals(stage, StringComparison.OrdinalIgnoreCase));
    }

    public static int GetDefaultProbability(string stage)
    {
        return stage.Trim() switch
        {
            var s when s.Equals(New, StringComparison.OrdinalIgnoreCase) => 10,
            var s when s.Equals(Discovery, StringComparison.OrdinalIgnoreCase) => 25,
            var s when s.Equals(Qualified, StringComparison.OrdinalIgnoreCase) => 50,
            var s when s.Equals(Proposal, StringComparison.OrdinalIgnoreCase) => 75,
            var s when s.Equals(Negotiation, StringComparison.OrdinalIgnoreCase) => 90,
            var s when s.Equals(Won, StringComparison.OrdinalIgnoreCase) => 100,
            var s when s.Equals(Lost, StringComparison.OrdinalIgnoreCase) => 0,
            _ => 10
        };
    }
}

public static class OpportunityStatus
{
    public const string Open = "Open";
    public const string Won = "Won";
    public const string Lost = "Lost";
    public const string Archived = "Archived";
}
