using BuildingBlocks.Domain.Models;

namespace Modules.Sales.Domain.Entities;

public class Opportunity : AuditableAggregateRoot
{
    private readonly List<Proposal> _proposals = new();

    public Guid LeadId { get; private set; }
    public string Stage { get; private set; } = string.Empty;
    public decimal EstimatedValue { get; private set; }
    public int Probability { get; private set; }
    public DateOnly ExpectedCloseDate { get; private set; }

    public IReadOnlyCollection<Proposal> Proposals => _proposals.AsReadOnly();

    private Opportunity() { }

    public Opportunity(Guid id, Guid leadId, string stage, decimal estimatedValue, int probability, DateOnly expectedCloseDate) : base(id)
    {
        LeadId = leadId;
        Stage = stage;
        EstimatedValue = estimatedValue;
        Probability = probability;
        ExpectedCloseDate = expectedCloseDate;
    }

    public static Opportunity Create(Guid leadId, string stage, decimal estimatedValue, int probability, DateOnly expectedCloseDate)
    {
        return new Opportunity(Guid.NewGuid(), leadId, stage, estimatedValue, probability, expectedCloseDate);
    }

    public void AddProposal(Proposal proposal)
    {
        _proposals.Add(proposal);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UpdateStage(string stage, int probability)
    {
        Stage = stage;
        Probability = probability;
        UpdateTimestamp(DateTime.UtcNow);
    }
}
