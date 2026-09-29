using Modules.CRM.Domain.Entities;
using Xunit;

namespace UnitTests;

public class SalesNoteDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializeSalesNoteCorrectly()
    {
        var leadId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        var note = SalesNote.Create(
            leadId,
            "Pre-sales Discovery",
            "Needs modern React + .NET web app",
            "Concerned about migration downtime",
            "Highlight zero-downtime deployment strategy",
            75,
            createdBy,
            companyId,
            contactId,
            "Competitor X",
            "$50k-$75k budget",
            "CTO is main decision maker"
        );

        Assert.NotEqual(Guid.Empty, note.Id);
        Assert.Equal(leadId, note.LeadId);
        Assert.Equal("Pre-sales Discovery", note.Title);
        Assert.Equal("Needs modern React + .NET web app", note.NeedAnalysis);
        Assert.Equal("Concerned about migration downtime", note.Objections);
        Assert.Equal("Highlight zero-downtime deployment strategy", note.Strategy);
        Assert.Equal(75, note.Probability);
        Assert.Equal(createdBy, note.CreatedBy);
        Assert.Equal(companyId, note.CompanyId);
        Assert.Equal(contactId, note.ContactId);
        Assert.Equal("Competitor X", note.CompetitorsMentioned);
        Assert.Equal("$50k-$75k budget", note.BudgetInformation);
        Assert.Equal("CTO is main decision maker", note.DecisionMakerInfo);
        Assert.False(note.IsDeleted);
    }

    [Fact]
    public void Create_WithEmptyLeadId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SalesNote.Create(
            Guid.Empty,
            "Title",
            "Needs",
            "Objections",
            "Strategy",
            50,
            Guid.NewGuid()
        ));
    }

    [Fact]
    public void Probability_ShouldBeClampedBetween0And100()
    {
        var leadId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        var noteHigh = SalesNote.Create(leadId, "High Prob", "Needs", "", "", 150, createdBy);
        var noteLow = SalesNote.Create(leadId, "Low Prob", "Needs", "", "", -20, createdBy);

        Assert.Equal(100, noteHigh.Probability);
        Assert.Equal(0, noteLow.Probability);
    }

    [Fact]
    public void UpdateInformation_ShouldUpdateFieldsAndSetUpdatedAt()
    {
        var note = SalesNote.Create(Guid.NewGuid(), "Initial Note", "Need A", "Obj A", "Strat A", 40, Guid.NewGuid());

        note.UpdateInformation("Updated Note", "Need B", "Obj B", "Strat B", 80);

        Assert.Equal("Updated Note", note.Title);
        Assert.Equal("Need B", note.NeedAnalysis);
        Assert.Equal("Obj B", note.Objections);
        Assert.Equal("Strat B", note.Strategy);
        Assert.Equal(80, note.Probability);
        Assert.NotNull(note.UpdatedAt);
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedAndDeletedAt()
    {
        var note = SalesNote.Create(Guid.NewGuid(), "Note", "Need", "Obj", "Strat", 50, Guid.NewGuid());
        var deleterId = Guid.NewGuid();

        note.SoftDelete(deleterId);

        Assert.True(note.IsDeleted);
        Assert.NotNull(note.DeletedAt);
        Assert.Equal(deleterId, note.DeletedBy);
    }
}
