using BuildingBlocks.Domain.Events;

namespace Modules.Support.Domain.Events;

public record TicketCreatedEvent(Guid TicketId, string TicketNumber, Guid CustomerId) : DomainEvent;
public record TicketAssignedEvent(Guid TicketId, Guid? AssignedToUserId) : DomainEvent;
public record TicketStatusChangedEvent(Guid TicketId, string OldStatus, string NewStatus) : DomainEvent;
public record TicketPriorityChangedEvent(Guid TicketId, string OldPriority, string NewPriority) : DomainEvent;
public record TicketResolvedEvent(Guid TicketId, string Resolution, DateTime ResolvedAt) : DomainEvent;
public record TicketClosedEvent(Guid TicketId, DateTime ClosedAt) : DomainEvent;
public record TicketReopenedEvent(Guid TicketId) : DomainEvent;
public record TicketCommentAddedEvent(Guid TicketId, Guid CommentId) : DomainEvent;
