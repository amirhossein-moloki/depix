using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Events;
using Xunit;

namespace UnitTests;

public class LeadDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializeLeadCorrectly()
    {
        var companyId = Guid.NewGuid();
        var lead = Lead.Create(companyId, "Website Redesign", "Website", "New corporate site", 50000m);

        Assert.NotEqual(Guid.Empty, lead.Id);
        Assert.Equal(companyId, lead.CompanyId);
        Assert.Equal("Website Redesign", lead.Title);
        Assert.Equal("Website", lead.Source);
        Assert.Equal("New corporate site", lead.Description);
        Assert.Equal(50000m, lead.EstimatedValue);
        Assert.Equal("NEW", lead.Status);
        Assert.Equal(0, lead.Score);
    }

    [Fact]
    public void Create_WithEmptyCompanyId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Lead.Create(Guid.Empty, "Invalid Lead", "Web"));
    }

    [Fact]
    public void Create_WithInvalidStatus_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Lead.Create(Guid.NewGuid(), "Title", "Web", status: "INVALID_STATUS"));
    }

    [Fact]
    public void ConvertToCustomer_ShouldUpdateStatusAndRaiseDomainEvents()
    {
        var companyId = Guid.NewGuid();
        var lead = Lead.Create(companyId, "Website Redesign", "Website");

        lead.ConvertToCustomer();

        Assert.Equal("CONVERTED", lead.Status);
        Assert.Equal(2, lead.DomainEvents.Count);
        Assert.Contains(lead.DomainEvents, e => e is LeadConvertedEvent);
        Assert.Contains(lead.DomainEvents, e => e is LeadStatusChangedEvent);
    }

    [Fact]
    public void Qualify_ShouldSetStatusToQualifiedAndEmitEvent()
    {
        var companyId = Guid.NewGuid();
        var lead = Lead.Create(companyId, "SEO Campaign", "Referral");

        lead.Qualify();

        Assert.Equal("QUALIFIED", lead.Status);
        Assert.Contains(lead.DomainEvents, e => e is LeadQualifiedEvent);
    }

    [Fact]
    public void Disqualify_WithValidReason_ShouldSetStatusAndReasonAndEmitEvent()
    {
        var companyId = Guid.NewGuid();
        var lead = Lead.Create(companyId, "E-commerce App", "Inbound Call");

        lead.Disqualify("Budget mismatch");

        Assert.Equal("DISQUALIFIED", lead.Status);
        Assert.Equal("Budget mismatch", lead.DisqualificationReason);
        Assert.Contains(lead.DomainEvents, e => e is LeadDisqualifiedEvent d && d.Reason == "Budget mismatch");
    }

    [Fact]
    public void Disqualify_WithoutReason_ShouldThrowArgumentException()
    {
        var lead = Lead.Create(Guid.NewGuid(), "Inbound Lead", "Web");

        Assert.Throws<ArgumentException>(() => lead.Disqualify(""));
    }

    [Fact]
    public void AssignToUser_ShouldSetAssignedToAndEmitEvent()
    {
        var lead = Lead.Create(Guid.NewGuid(), "ERP Integration", "Email");
        var userId = Guid.NewGuid();

        lead.AssignToUser(userId);

        Assert.Equal(userId, lead.AssignedTo);
        Assert.Contains(lead.DomainEvents, e => e is LeadAssignedEvent a && a.AssignedTo == userId);
    }

    [Fact]
    public void ChangeStatus_WithValidTargetStatus_ShouldUpdateStatusAndEmitEvent()
    {
        var lead = Lead.Create(Guid.NewGuid(), "Mobile App", "Social Media");

        lead.ChangeStatus("CONTACTED");

        Assert.Equal("CONTACTED", lead.Status);
        Assert.Contains(lead.DomainEvents, e => e is LeadStatusChangedEvent s && s.OldStatus == "NEW" && s.NewStatus == "CONTACTED");
    }

    [Fact]
    public void ChangeStatus_WithInvalidStatus_ShouldThrowArgumentException()
    {
        var lead = Lead.Create(Guid.NewGuid(), "Mobile App", "Social Media");

        Assert.Throws<ArgumentException>(() => lead.ChangeStatus("UNKNOWN_STATUS"));
    }

    [Fact]
    public void UpdateScore_WithNegativeScore_ShouldThrowArgumentException()
    {
        var lead = Lead.Create(Guid.NewGuid(), "Portal", "Web");

        Assert.Throws<ArgumentException>(() => lead.UpdateScore(-10));
    }
}
