using BuildingBlocks.Domain.ValueObjects;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Events;
using Xunit;

namespace UnitTests;

public class OpportunityDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateOpportunityAndRaiseCreatedEvent()
    {
        // Arrange
        var title = "E-Commerce Website Development";
        var leadId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var value = Money.Create(15000, "USD");

        // Act
        var opportunity = Opportunity.Create(
            title: title,
            leadId: leadId,
            companyId: companyId,
            value: value,
            stage: OpportunityStage.Discovery
        );

        // Assert
        Assert.NotNull(opportunity);
        Assert.Equal(title, opportunity.Title);
        Assert.Equal(leadId, opportunity.LeadId);
        Assert.Equal(companyId, opportunity.CompanyId);
        Assert.Equal(15000, opportunity.Value.Amount);
        Assert.Equal("USD", opportunity.Value.Currency);
        Assert.Equal(OpportunityStage.Discovery, opportunity.Stage);
        Assert.Equal(OpportunityStatus.Open, opportunity.Status);
        Assert.Equal(25, opportunity.Probability); // Stage default for Discovery

        var createdEvent = Assert.Single(opportunity.DomainEvents.OfType<OpportunityCreatedEvent>());
        Assert.Equal(opportunity.Id, createdEvent.OpportunityId);
        Assert.Equal(title, createdEvent.Title);
    }

    [Fact]
    public void Create_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Opportunity.Create(title: "   "));
    }

    [Fact]
    public void Create_WithInvalidProbability_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Opportunity.Create(title: "Website Deal", probability: 120));
    }

    [Fact]
    public void MoveToStage_ValidStage_ShouldUpdateStageAndRaiseEvent()
    {
        // Arrange
        var opportunity = Opportunity.Create("Corporate Site", stage: OpportunityStage.New);

        // Act
        opportunity.MoveToStage(OpportunityStage.Proposal);

        // Assert
        Assert.Equal(OpportunityStage.Proposal, opportunity.Stage);
        Assert.Equal(75, opportunity.Probability);
        Assert.Contains(opportunity.DomainEvents, e => e is OpportunityStageChangedEvent);
    }

    [Fact]
    public void MoveToStage_InvalidStage_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var opportunity = Opportunity.Create("Corporate Site");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => opportunity.MoveToStage("NonExistentStage"));
    }

    [Fact]
    public void MarkAsWon_ShouldSetWonStatusProbabilityAndRaiseEvent()
    {
        // Arrange
        var opportunity = Opportunity.Create("App Redesign", value: Money.Create(20000, "USD"));

        // Act
        opportunity.MarkAsWon();

        // Assert
        Assert.Equal(OpportunityStage.Won, opportunity.Stage);
        Assert.Equal(OpportunityStatus.Won, opportunity.Status);
        Assert.Equal(100, opportunity.Probability);
        Assert.NotNull(opportunity.WonAt);
        Assert.Null(opportunity.LostAt);

        var wonEvent = Assert.Single(opportunity.DomainEvents.OfType<OpportunityWonEvent>());
        Assert.Equal(opportunity.Id, wonEvent.OpportunityId);
        Assert.Equal(20000, wonEvent.Value.Amount);
    }

    [Fact]
    public void MarkAsLost_WithoutReason_ShouldThrowArgumentException()
    {
        // Arrange
        var opportunity = Opportunity.Create("Maintenance Contract");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => opportunity.MarkAsLost(lossReason: "  "));
    }

    [Fact]
    public void MarkAsLost_WithValidReason_ShouldSetLostStatusAndRaiseEvent()
    {
        // Arrange
        var opportunity = Opportunity.Create("Maintenance Contract");
        var reason = "Price too high compared to competitors";

        // Act
        opportunity.MarkAsLost(reason);

        // Assert
        Assert.Equal(OpportunityStage.Lost, opportunity.Stage);
        Assert.Equal(OpportunityStatus.Lost, opportunity.Status);
        Assert.Equal(0, opportunity.Probability);
        Assert.Equal(reason, opportunity.LossReason);
        Assert.NotNull(opportunity.LostAt);

        var lostEvent = Assert.Single(opportunity.DomainEvents.OfType<OpportunityLostEvent>());
        Assert.Equal(opportunity.Id, lostEvent.OpportunityId);
        Assert.Equal(reason, lostEvent.LossReason);
    }

    [Fact]
    public void Reopen_LostOpportunity_ShouldResetToOpen()
    {
        // Arrange
        var opportunity = Opportunity.Create("Redesign Portal");
        opportunity.MarkAsLost("Budget frozen");

        // Act
        opportunity.Reopen(OpportunityStage.Discovery);

        // Assert
        Assert.Equal(OpportunityStage.Discovery, opportunity.Stage);
        Assert.Equal(OpportunityStatus.Open, opportunity.Status);
        Assert.Equal(25, opportunity.Probability);
        Assert.Null(opportunity.WonAt);
        Assert.Null(opportunity.LostAt);
        Assert.Null(opportunity.LossReason);
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedTrueAndArchivedStatus()
    {
        // Arrange
        var opportunity = Opportunity.Create("Old Deal");
        var userId = Guid.NewGuid();

        // Act
        opportunity.SoftDelete(userId);

        // Assert
        Assert.True(opportunity.IsDeleted);
        Assert.NotNull(opportunity.DeletedAt);
        Assert.Equal(userId, opportunity.DeletedBy);
        Assert.Equal(OpportunityStatus.Archived, opportunity.Status);
    }
}
