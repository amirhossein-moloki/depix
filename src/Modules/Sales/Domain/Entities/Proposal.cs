using BuildingBlocks.Domain.Models;

namespace Modules.Sales.Domain.Entities;

public class Proposal : AuditableEntity
{
    private readonly List<ProposalItem> _proposalItems = new();

    public Guid OpportunityId { get; private set; }
    public string Version { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public decimal Discount { get; private set; }
    public DateOnly ValidUntil { get; private set; }
    public string Status { get; private set; } = string.Empty;

    public IReadOnlyCollection<ProposalItem> ProposalItems => _proposalItems.AsReadOnly();

    private Proposal() { }

    public Proposal(Guid id, Guid opportunityId, string version, decimal amount, decimal discount, DateOnly validUntil, string status) : base(id)
    {
        OpportunityId = opportunityId;
        Version = version;
        Amount = amount;
        Discount = discount;
        ValidUntil = validUntil;
        Status = status;
    }

    public static Proposal Create(Guid opportunityId, string version, decimal amount, decimal discount, DateOnly validUntil, string status = "DRAFT")
    {
        return new Proposal(Guid.NewGuid(), opportunityId, version, amount, discount, validUntil, status);
    }

    public void AddItem(ProposalItem item)
    {
        _proposalItems.Add(item);
        UpdateTimestamp(DateTime.UtcNow);
    }
}
