using BuildingBlocks.Domain.ValueObjects;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Events;
using Xunit;

namespace UnitTests;

public class ProposalDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateProposal()
    {
        var opportunityId = Guid.NewGuid();
        var title = "Website Development Proposal";
        var validUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var proposal = Proposal.Create(opportunityId, title, validUntil, version: "1.0", description: "Desc", currency: "USD");

        Assert.NotNull(proposal);
        Assert.NotEqual(Guid.Empty, proposal.Id);
        Assert.Equal(opportunityId, proposal.OpportunityId);
        Assert.Equal(title, proposal.Title);
        Assert.Equal("1.0", proposal.Version);
        Assert.Equal(ProposalStatus.Draft, proposal.Status);
        Assert.Equal(0m, proposal.Total.Amount);
        Assert.Single(proposal.DomainEvents);
        Assert.IsType<ProposalCreatedEvent>(proposal.DomainEvents.First());
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        var opportunityId = Guid.NewGuid();
        var validUntil = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        Assert.Throws<ArgumentException>(() => Proposal.Create(opportunityId, "", validUntil));
    }

    [Fact]
    public void AddItem_ShouldAddProposalItemAndRecalculateTotals()
    {
        var proposal = Proposal.Create(Guid.NewGuid(), "Test Proposal", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));

        proposal.AddItem("Custom Design", "UI/UX design", 1, Money.Create(1000m, "USD"), Money.Create(100m, "USD"));
        proposal.AddItem("Backend Development", "API development", 2, Money.Create(500m, "USD"), Money.Create(0m, "USD"));

        Assert.Equal(2, proposal.ProposalItems.Count);
        Assert.Equal(2000m, proposal.Subtotal.Amount);
        Assert.Equal(100m, proposal.Discount.Amount);
        Assert.Equal(1900m, proposal.Total.Amount);
    }

    [Fact]
    public void UpdateItem_ShouldUpdateProposalItemAndRecalculateTotals()
    {
        var proposal = Proposal.Create(Guid.NewGuid(), "Test Proposal", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        proposal.AddItem("Item 1", "Desc", 1, Money.Create(500m, "USD"));

        var item = proposal.ProposalItems.First();
        proposal.UpdateItem(item.Id, "Item 1 Updated", "Desc updated", 2, Money.Create(600m, "USD"), Money.Create(50m, "USD"));

        Assert.Single(proposal.ProposalItems);
        Assert.Equal(1200m, proposal.Subtotal.Amount);
        Assert.Equal(50m, proposal.Discount.Amount);
        Assert.Equal(1150m, proposal.Total.Amount);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveProposalItemAndRecalculateTotals()
    {
        var proposal = Proposal.Create(Guid.NewGuid(), "Test Proposal", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        proposal.AddItem("Item 1", "Desc", 1, Money.Create(500m, "USD"));
        proposal.AddItem("Item 2", "Desc", 1, Money.Create(300m, "USD"));

        var itemToRemove = proposal.ProposalItems.First(i => i.Name == "Item 1");
        proposal.RemoveItem(itemToRemove.Id);

        Assert.Single(proposal.ProposalItems);
        Assert.Equal(300m, proposal.Total.Amount);
    }

    [Fact]
    public void ChangeStatus_ToAccepted_ShouldPublishAcceptedEvent()
    {
        var proposal = Proposal.Create(Guid.NewGuid(), "Test Proposal", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        proposal.ClearDomainEvents();

        proposal.ChangeStatus(ProposalStatus.Accepted);

        Assert.Equal(ProposalStatus.Accepted, proposal.Status);
        Assert.Single(proposal.DomainEvents);
        Assert.IsType<ProposalAcceptedEvent>(proposal.DomainEvents.First());
    }

    [Fact]
    public void ChangeStatus_ToRejected_ShouldPublishRejectedEvent()
    {
        var proposal = Proposal.Create(Guid.NewGuid(), "Test Proposal", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        proposal.ClearDomainEvents();

        proposal.ChangeStatus(ProposalStatus.Rejected, "Too expensive");

        Assert.Equal(ProposalStatus.Rejected, proposal.Status);
        Assert.Single(proposal.DomainEvents);
        var evt = Assert.IsType<ProposalRejectedEvent>(proposal.DomainEvents.First());
        Assert.Equal("Too expensive", evt.Reason);
    }

    [Fact]
    public void ChangeStatus_InvalidStatus_ShouldThrowInvalidOperationException()
    {
        var proposal = Proposal.Create(Guid.NewGuid(), "Test Proposal", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));

        Assert.Throws<InvalidOperationException>(() => proposal.ChangeStatus("INVALID_STATUS"));
    }

    [Fact]
    public void ProposalItem_InvalidQuantity_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ProposalItem.Create(Guid.NewGuid(), "Name", "Desc", 0, Money.Create(100m, "USD")));
    }
}
