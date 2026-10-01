namespace Modules.Support.Application.DTOs;

public record TicketCommentDto(
    Guid Id,
    Guid TicketId,
    Guid? AuthorUserId,
    string AuthorName,
    string Message,
    string CommentType,
    DateTime CreatedAt
);

public record TicketDto(
    Guid Id,
    string TicketNumber,
    Guid CustomerId,
    Guid? ProjectId,
    Guid? ContactId,
    string Subject,
    string Description,
    string Status,
    string Priority,
    string Category,
    Guid? AssignedToUserId,
    DateTime OpenedAt,
    DateTime? DueAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    string? Resolution,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record TicketListItemDto(
    Guid Id,
    string TicketNumber,
    Guid CustomerId,
    Guid? ProjectId,
    Guid? ContactId,
    string Subject,
    string Status,
    string Priority,
    string Category,
    Guid? AssignedToUserId,
    DateTime OpenedAt,
    DateTime? DueAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    int CommentCount
);

public record TicketDetailDto(
    Guid Id,
    string TicketNumber,
    Guid CustomerId,
    Guid? ProjectId,
    Guid? ContactId,
    string Subject,
    string Description,
    string Status,
    string Priority,
    string Category,
    Guid? AssignedToUserId,
    DateTime OpenedAt,
    DateTime? DueAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    string? Resolution,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<TicketCommentDto> Comments
);

public record CreateTicketRequest(
    Guid CustomerId,
    Guid? ProjectId,
    Guid? ContactId,
    string Subject,
    string Description,
    string Priority,
    string Category,
    Guid? AssignedToUserId,
    DateTime? DueAt
);

public record UpdateTicketRequest(
    string Subject,
    string Description,
    string Category,
    Guid? ProjectId,
    Guid? ContactId,
    DateTime? DueAt
);

public record AssignTicketRequest(
    Guid? AssignedToUserId
);

public record ChangeTicketPriorityRequest(
    string Priority
);

public record ChangeTicketStatusRequest(
    string Status
);

public record ResolveTicketRequest(
    string Resolution,
    DateTime? ResolvedAt
);

public record ReopenTicketRequest(
    string? Reason
);

public record CancelTicketRequest(
    string? Reason
);

public record AddTicketCommentRequest(
    string Message
);
