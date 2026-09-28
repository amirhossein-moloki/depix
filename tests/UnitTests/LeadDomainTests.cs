using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Events;
using Xunit;

namespace UnitTests;

public class LeadDomainTests
{
    [Fact]
    public void ConvertToCustomer_ShouldUpdateStatusAndRaiseDomainEvent()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var lead = Lead.Create(companyId, "Website");

        // Act
        lead.ConvertToCustomer();

        // Assert
        Assert.Equal("CONVERTED", lead.Status);
        Assert.Single(lead.DomainEvents);

        var domainEvent = Assert.IsType<LeadConvertedEvent>(lead.DomainEvents.First());
        Assert.Equal(lead.Id, domainEvent.LeadId);
        Assert.Equal(companyId, domainEvent.CompanyId);
    }

    [Fact]
    public void Qualify_ShouldSetStatusToQualified()
    {
        var lead = Lead.Create(Guid.NewGuid(), "Referral");

        lead.Qualify();

        Assert.Equal("QUALIFIED", lead.Status);
    }
}
