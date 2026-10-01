using BuildingBlocks.Common.Exceptions;
using Modules.Support.Domain.Constants;
using Modules.Support.Domain.Entities;
using Modules.Support.Domain.Events;
using Xunit;

namespace UnitTests;

public class SupportDomainTests
{
    [Fact]
    public void CreateTicket_WithValidData_ShouldInitializeTicketCorrectly()
    {
        var customerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        var ticket = Ticket.Create(
            "TICK-202610-0001",
            customerId,
            projectId,
            contactId,
            "System Issue",
            "Cannot access dashboard",
            TicketPriority.High,
            TicketCategory.TechnicalIssue);

        Assert.NotNull(ticket);
        Assert.Equal("TICK-202610-0001", ticket.TicketNumber);
        Assert.Equal(customerId, ticket.CustomerId);
        Assert.Equal(projectId, ticket.ProjectId);
        Assert.Equal(contactId, ticket.ContactId);
        Assert.Equal("System Issue", ticket.Subject);
        Assert.Equal("Cannot access dashboard", ticket.Description);
        Assert.Equal(TicketPriority.High, ticket.Priority);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal(TicketCategory.TechnicalIssue, ticket.Category);
        Assert.Null(ticket.AssignedToUserId);
        Assert.Null(ticket.ResolvedAt);
        Assert.Null(ticket.ClosedAt);
        Assert.Null(ticket.Resolution);
        Assert.False(ticket.IsDeleted);

        Assert.Single(ticket.DomainEvents);
        Assert.IsType<TicketCreatedEvent>(ticket.DomainEvents.First());
    }

    [Fact]
    public void CreateTicket_WithEmptyCustomerId_ShouldThrowBusinessRuleException()
    {
        var ex = Assert.Throws<BusinessRuleException>(() => Ticket.Create(
            "TICK-0001",
            Guid.Empty,
            null,
            null,
            "Subject",
            "Description"));

        Assert.Contains("valid customer", ex.Message);
    }

    [Fact]
    public void CreateTicket_WithEmptySubject_ShouldThrowBusinessRuleException()
    {
        var ex = Assert.Throws<BusinessRuleException>(() => Ticket.Create(
            "TICK-0001",
            Guid.NewGuid(),
            null,
            null,
            "",
            "Description"));

        Assert.Contains("subject is required", ex.Message);
    }

    [Fact]
    public void CreateTicket_WithEmptyDescription_ShouldThrowBusinessRuleException()
    {
        var ex = Assert.Throws<BusinessRuleException>(() => Ticket.Create(
            "TICK-0001",
            Guid.NewGuid(),
            null,
            null,
            "Subject",
            " "));

        Assert.Contains("description is required", ex.Message);
    }

    [Fact]
    public void CreateTicket_WithInvalidPriority_ShouldThrowBusinessRuleException()
    {
        var ex = Assert.Throws<BusinessRuleException>(() => Ticket.Create(
            "TICK-0001",
            Guid.NewGuid(),
            null,
            null,
            "Subject",
            "Description",
            priority: "SUPER_HIGH"));

        Assert.Contains("Invalid ticket priority", ex.Message);
    }

    [Fact]
    public void CreateTicket_WithInvalidCategory_ShouldThrowBusinessRuleException()
    {
        var ex = Assert.Throws<BusinessRuleException>(() => Ticket.Create(
            "TICK-0001",
            Guid.NewGuid(),
            null,
            null,
            "Subject",
            "Description",
            category: "UNKNOWN_CATEGORY"));

        Assert.Contains("Invalid ticket category", ex.Message);
    }

    [Fact]
    public void AssignTo_ShouldUpdateAssignedToUserIdAndAddCommentAndEvent()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        var userId = Guid.NewGuid();

        ticket.AssignTo(userId, "AdminUser");

        Assert.Equal(userId, ticket.AssignedToUserId);
        Assert.Single(ticket.Comments);
        Assert.Equal(CommentType.Assignment, ticket.Comments.First().CommentType);
        Assert.Contains(userId.ToString(), ticket.Comments.First().Message);
        Assert.Contains(ticket.DomainEvents, e => e is TicketAssignedEvent);
    }

    [Fact]
    public void ChangePriority_ShouldUpdatePriorityAndAddCommentAndEvent()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description", priority: TicketPriority.Low);

        ticket.ChangePriority(TicketPriority.Urgent, updatedByName: "Agent");

        Assert.Equal(TicketPriority.Urgent, ticket.Priority);
        Assert.Single(ticket.Comments);
        Assert.Equal(CommentType.PriorityChange, ticket.Comments.First().CommentType);
        Assert.Contains(ticket.DomainEvents, e => e is TicketPriorityChangedEvent);
    }

    [Fact]
    public void ChangePriority_WithInvalidPriority_ShouldThrowBusinessRuleException()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");

        var ex = Assert.Throws<BusinessRuleException>(() => ticket.ChangePriority("EXTREME"));
        Assert.Contains("Invalid ticket priority", ex.Message);
    }

    [Fact]
    public void ChangeStatus_WithValidTransition_ShouldUpdateStatusAndAddCommentAndEvent()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");

        ticket.ChangeStatus(TicketStatus.InProgress, updatedByName: "Agent");

        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Single(ticket.Comments);
        Assert.Contains(ticket.DomainEvents, e => e is TicketStatusChangedEvent);
    }

    [Fact]
    public void ChangeStatus_WithInvalidStatus_ShouldThrowBusinessRuleException()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");

        var ex = Assert.Throws<BusinessRuleException>(() => ticket.ChangeStatus("INVALID_STATUS"));
        Assert.Contains("Invalid ticket status", ex.Message);
    }

    [Fact]
    public void ResolveTicket_WithResolution_ShouldUpdateStatusToResolvedAndSetResolvedAt()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");

        ticket.Resolve("Fixed database connection pooling.", resolvedByName: "DevSupport");

        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.NotNull(ticket.ResolvedAt);
        Assert.Equal("Fixed database connection pooling.", ticket.Resolution);
        Assert.Contains(ticket.DomainEvents, e => e is TicketResolvedEvent);
    }

    [Fact]
    public void ResolveTicket_WithoutResolution_ShouldThrowBusinessRuleException()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");

        var ex = Assert.Throws<BusinessRuleException>(() => ticket.Resolve(""));
        Assert.Contains("Resolution details are required", ex.Message);
    }

    [Fact]
    public void CloseTicket_ShouldUpdateStatusToClosedAndSetClosedAt()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        ticket.Resolve("Resolved issue", resolvedByName: "Support");

        ticket.Close(closedByName: "Support");

        Assert.Equal(TicketStatus.Closed, ticket.Status);
        Assert.NotNull(ticket.ClosedAt);
        Assert.Contains(ticket.DomainEvents, e => e is TicketClosedEvent);
    }

    [Fact]
    public void CloseTicket_WhenCancelled_ShouldThrowBusinessRuleException()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        ticket.Cancel("Duplicate request");

        var ex = Assert.Throws<BusinessRuleException>(() => ticket.Close());
        Assert.Contains("Cannot close a cancelled ticket", ex.Message);
    }

    [Fact]
    public void CancelTicket_ShouldUpdateStatusToCancelled()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");

        ticket.Cancel("Customer withdrew request", cancelledByName: "Agent");

        Assert.Equal(TicketStatus.Cancelled, ticket.Status);
        Assert.Single(ticket.Comments);
        Assert.Contains("Customer withdrew request", ticket.Comments.First().Message);
    }

    [Fact]
    public void CancelTicket_WhenClosed_ShouldThrowBusinessRuleException()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        ticket.Resolve("Resolved");
        ticket.Close();

        var ex = Assert.Throws<BusinessRuleException>(() => ticket.Cancel("Too late"));
        Assert.Contains("Cannot cancel a closed ticket", ex.Message);
    }

    [Fact]
    public void ReopenTicket_WhenResolvedOrClosed_ShouldUpdateStatusToOpenAndClearTimestamps()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        ticket.Resolve("Fixed issue");
        ticket.Close();

        ticket.Reopen("Issue recurred", reopenedByName: "Customer");

        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Null(ticket.ResolvedAt);
        Assert.Null(ticket.ClosedAt);
        Assert.Contains(ticket.DomainEvents, e => e is TicketReopenedEvent);
    }

    [Fact]
    public void ReopenTicket_WhenCancelled_ShouldThrowBusinessRuleException()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        ticket.Cancel("Cancelled");

        var ex = Assert.Throws<BusinessRuleException>(() => ticket.Reopen("Try again"));
        Assert.Contains("Cancelled tickets cannot be reopened", ex.Message);
    }

    [Fact]
    public void AddComment_WithValidMessage_ShouldAddTicketCommentAndEvent()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        var authorId = Guid.NewGuid();

        var comment = ticket.AddComment(authorId, "John Support", "Checking log files now.");

        Assert.NotNull(comment);
        Assert.Single(ticket.Comments);
        Assert.Equal("Checking log files now.", comment.Message);
        Assert.Equal("John Support", comment.AuthorName);
        Assert.Equal(authorId, comment.AuthorUserId);
        Assert.Contains(ticket.DomainEvents, e => e is TicketCommentAddedEvent);
    }

    [Fact]
    public void AddComment_ToCancelledTicket_ShouldThrowBusinessRuleException()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        ticket.Cancel();

        var ex = Assert.Throws<BusinessRuleException>(() => ticket.AddComment(null, "Agent", "New comment"));
        Assert.Contains("Cannot add comment to a cancelled ticket", ex.Message);
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedToTrueAndSetDeletedAt()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        var deleterId = Guid.NewGuid();

        ticket.SoftDelete(deleterId);

        Assert.True(ticket.IsDeleted);
        Assert.NotNull(ticket.DeletedAt);
        Assert.Equal(deleterId, ticket.DeletedBy);
    }
}
