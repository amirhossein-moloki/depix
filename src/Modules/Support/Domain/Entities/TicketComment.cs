using BuildingBlocks.Domain.Models;
using Modules.Support.Domain.Constants;

namespace Modules.Support.Domain.Entities;

public class TicketComment : AuditableEntity
{
    public Guid TicketId { get; private set; }
    public Guid? AuthorUserId { get; private set; }
    public string AuthorName { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string CommentType { get; private set; } = Constants.CommentType.Comment;

    private TicketComment() { }

    internal TicketComment(Guid id, Guid ticketId, Guid? authorUserId, string authorName, string message, string commentType = Constants.CommentType.Comment)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Comment message cannot be empty.", nameof(message));

        TicketId = ticketId;
        AuthorUserId = authorUserId;
        AuthorName = authorName ?? string.Empty;
        Message = message;
        CommentType = string.IsNullOrWhiteSpace(commentType) ? Constants.CommentType.Comment : commentType;
    }

    public static TicketComment Create(Guid ticketId, Guid? authorUserId, string authorName, string message, string commentType = Constants.CommentType.Comment)
    {
        return new TicketComment(Guid.NewGuid(), ticketId, authorUserId, authorName, message, commentType);
    }
}
