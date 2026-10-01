using Modules.Support.Application.DTOs;
using Modules.Support.Domain.Entities;

namespace Modules.Support.Application.Mappings;

public static class SupportMappingExtensions
{
    public static TicketDto ToDto(this Ticket ticket)
    {
        return new TicketDto(
            ticket.Id,
            ticket.TicketNumber,
            ticket.CustomerId,
            ticket.ProjectId,
            ticket.ContactId,
            ticket.Subject,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.Category,
            ticket.AssignedToUserId,
            ticket.OpenedAt,
            ticket.DueAt,
            ticket.ResolvedAt,
            ticket.ClosedAt,
            ticket.Resolution,
            ticket.CreatedAt,
            ticket.UpdatedAt
        );
    }

    public static TicketListItemDto ToListItemDto(this Ticket ticket)
    {
        return new TicketListItemDto(
            ticket.Id,
            ticket.TicketNumber,
            ticket.CustomerId,
            ticket.ProjectId,
            ticket.ContactId,
            ticket.Subject,
            ticket.Status,
            ticket.Priority,
            ticket.Category,
            ticket.AssignedToUserId,
            ticket.OpenedAt,
            ticket.DueAt,
            ticket.ResolvedAt,
            ticket.ClosedAt,
            ticket.Comments.Count
        );
    }

    public static TicketDetailDto ToDetailDto(this Ticket ticket)
    {
        return new TicketDetailDto(
            ticket.Id,
            ticket.TicketNumber,
            ticket.CustomerId,
            ticket.ProjectId,
            ticket.ContactId,
            ticket.Subject,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.Category,
            ticket.AssignedToUserId,
            ticket.OpenedAt,
            ticket.DueAt,
            ticket.ResolvedAt,
            ticket.ClosedAt,
            ticket.Resolution,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.Comments.Select(c => c.ToDto()).ToList()
        );
    }

    public static TicketCommentDto ToDto(this TicketComment comment)
    {
        return new TicketCommentDto(
            comment.Id,
            comment.TicketId,
            comment.AuthorUserId,
            comment.AuthorName,
            comment.Message,
            comment.CommentType,
            comment.CreatedAt
        );
    }
}
