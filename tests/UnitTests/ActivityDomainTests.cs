using Modules.CRM.Domain.Entities;
using Xunit;

namespace UnitTests;

public class ActivityDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializeActivityCorrectly()
    {
        var leadId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow.AddHours(-1);
        var followUpAt = DateTime.UtcNow.AddDays(2);

        var activity = Activity.Create(
            leadId,
            userId,
            "CALL",
            "Introductory Call",
            "Discussed website requirements",
            "Successful",
            85,
            contactId,
            companyId,
            occurredAt,
            followUpAt,
            "Send proposal next week"
        );

        Assert.NotEqual(Guid.Empty, activity.Id);
        Assert.Equal(leadId, activity.LeadId);
        Assert.Equal(userId, activity.UserId);
        Assert.Equal("CALL", activity.Type);
        Assert.Equal("Introductory Call", activity.Subject);
        Assert.Equal("Discussed website requirements", activity.Description);
        Assert.Equal("Successful", activity.Result);
        Assert.Equal(85, activity.QualityScore);
        Assert.Equal(contactId, activity.ContactId);
        Assert.Equal(companyId, activity.CompanyId);
        Assert.Equal(occurredAt, activity.OccurredAt);
        Assert.Equal(followUpAt, activity.FollowUpAt);
        Assert.Equal("Send proposal next week", activity.FollowUpNotes);
        Assert.True(activity.IsFollowUpRequired);
        Assert.False(activity.IsDeleted);
    }

    [Fact]
    public void Create_WithEmptyLeadId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Activity.Create(
            Guid.Empty,
            Guid.NewGuid(),
            "MEETING",
            "Demo Meeting",
            "Description",
            "Result",
            80
        ));
    }

    [Fact]
    public void UpdateInformation_ShouldUpdatePropertiesAndSetUpdatedAt()
    {
        var leadId = Guid.NewGuid();
        var activity = Activity.Create(leadId, Guid.NewGuid(), "EMAIL", "Initial Email", "Sent email", "Pending", 50);

        var newOccurredAt = DateTime.UtcNow;
        var newFollowUp = DateTime.UtcNow.AddDays(5);

        activity.UpdateInformation(
            "MEETING",
            "Follow-up Meeting",
            "Met with client",
            "MeetingScheduled",
            90,
            null,
            null,
            newOccurredAt,
            newFollowUp,
            "Prepare slides"
        );

        Assert.Equal("MEETING", activity.Type);
        Assert.Equal("Follow-up Meeting", activity.Subject);
        Assert.Equal("Met with client", activity.Description);
        Assert.Equal("MeetingScheduled", activity.Result);
        Assert.Equal(90, activity.QualityScore);
        Assert.NotNull(activity.UpdatedAt);
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedAndDeletedAt()
    {
        var activity = Activity.Create(Guid.NewGuid(), Guid.NewGuid(), "CALL", "Call", "Desc", "Result", 60);
        var deleterId = Guid.NewGuid();

        activity.SoftDelete(deleterId);

        Assert.True(activity.IsDeleted);
        Assert.NotNull(activity.DeletedAt);
        Assert.Equal(deleterId, activity.DeletedBy);
    }
}
