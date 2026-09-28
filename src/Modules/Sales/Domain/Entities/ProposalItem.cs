using BuildingBlocks.Domain.Models;

namespace Modules.Sales.Domain.Entities;

public class ProposalItem : Entity
{
    public Guid ProposalId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }

    private ProposalItem() { }

    public ProposalItem(Guid id, Guid proposalId, string name, string description, decimal price) : base(id)
    {
        ProposalId = proposalId;
        Name = name;
        Description = description;
        Price = price;
    }

    public static ProposalItem Create(Guid proposalId, string name, string description, decimal price)
    {
        return new ProposalItem(Guid.NewGuid(), proposalId, name, description, price);
    }
}
