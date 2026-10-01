using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Support.Application.DTOs;
using Modules.Support.Application.Mappings;
using Modules.Support.Application.Services;
using Modules.Support.Domain.Constants;
using Modules.Support.Domain.Entities;
using Modules.Support.Domain.Repositories;

namespace Modules.Support.Application.Commands;

public record CreateTicketCommand(
    Guid CustomerId,
    Guid? ProjectId,
    Guid? ContactId,
    string Subject,
    string Description,
    string Priority,
    string Category,
    Guid? AssignedToUserId,
    DateTime? DueAt) : ICommand<TicketDetailDto>;

public record UpdateTicketCommand(
    Guid TicketId,
    string Subject,
    string Description,
    string Category,
    Guid? ProjectId,
    Guid? ContactId,
    DateTime? DueAt) : ICommand<TicketDetailDto>;

public record AssignTicketCommand(
    Guid TicketId,
    Guid? AssignedToUserId,
    string? AssignedByName = null) : ICommand<TicketDetailDto>;

public record ChangeTicketPriorityCommand(
    Guid TicketId,
    string Priority,
    string? UpdatedByName = null) : ICommand<TicketDetailDto>;

public record ChangeTicketStatusCommand(
    Guid TicketId,
    string Status,
    string? UpdatedByName = null) : ICommand<TicketDetailDto>;

public record ResolveTicketCommand(
    Guid TicketId,
    string Resolution,
    DateTime? ResolvedAt = null,
    string? ResolvedByName = null) : ICommand<TicketDetailDto>;

public record CloseTicketCommand(
    Guid TicketId,
    string? ClosedByName = null) : ICommand<TicketDetailDto>;

public record CancelTicketCommand(
    Guid TicketId,
    string? Reason = null,
    string? CancelledByName = null) : ICommand<TicketDetailDto>;

public record ReopenTicketCommand(
    Guid TicketId,
    string? Reason = null,
    string? ReopenedByName = null) : ICommand<TicketDetailDto>;

public record DeleteTicketCommand(
    Guid TicketId) : ICommand<bool>;

public record AddTicketCommentCommand(
    Guid TicketId,
    Guid? AuthorUserId,
    string AuthorName,
    string Message) : ICommand<TicketCommentDto>;

public class CreateTicketCommandHandler : ICommandHandler<CreateTicketCommand, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketNumberGenerator _numberGenerator;
    private readonly ICustomerService _customerService;
    private readonly IProjectService _projectService;
    private readonly ICustomerContactService _contactService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        ITicketNumberGenerator numberGenerator,
        ICustomerService customerService,
        IProjectService projectService,
        ICustomerContactService contactService,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _numberGenerator = numberGenerator;
        _customerService = customerService;
        _projectService = projectService;
        _contactService = contactService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDetailDto> HandleAsync(CreateTicketCommand command, CancellationToken cancellationToken = default)
    {
        var customerExists = await _customerService.CustomerExistsAsync(command.CustomerId, cancellationToken);
        if (!customerExists)
            throw new EntityNotFoundException("Customer", command.CustomerId);

        if (command.ProjectId.HasValue)
        {
            var projectExists = await _projectService.ProjectExistsAsync(command.ProjectId.Value, cancellationToken);
            if (!projectExists)
                throw new EntityNotFoundException("Project", command.ProjectId.Value);
        }

        if (command.ContactId.HasValue)
        {
            var contact = await _contactService.GetContactByIdAsync(command.ContactId.Value, cancellationToken);
            if (contact == null)
                throw new EntityNotFoundException("Contact", command.ContactId.Value);
        }

        var ticketNumber = await _numberGenerator.GenerateTicketNumberAsync(cancellationToken);

        var ticket = Ticket.Create(
            ticketNumber,
            command.CustomerId,
            command.ProjectId,
            command.ContactId,
            command.Subject,
            command.Description,
            command.Priority,
            command.Category,
            command.AssignedToUserId,
            command.DueAt);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }
}

public class UpdateTicketCommandHandler : ICommandHandler<UpdateTicketCommand, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IProjectService _projectService;
    private readonly ICustomerContactService _contactService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTicketCommandHandler(
        ITicketRepository ticketRepository,
        IProjectService projectService,
        ICustomerContactService contactService,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _projectService = projectService;
        _contactService = contactService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDetailDto> HandleAsync(UpdateTicketCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        if (command.ProjectId.HasValue)
        {
            var projectExists = await _projectService.ProjectExistsAsync(command.ProjectId.Value, cancellationToken);
            if (!projectExists)
                throw new EntityNotFoundException("Project", command.ProjectId.Value);
        }

        if (command.ContactId.HasValue)
        {
            var contact = await _contactService.GetContactByIdAsync(command.ContactId.Value, cancellationToken);
            if (contact == null)
                throw new EntityNotFoundException("Contact", command.ContactId.Value);
        }

        ticket.UpdateDetails(
            command.Subject,
            command.Description,
            command.Category,
            command.ProjectId,
            command.ContactId,
            command.DueAt);

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }
}

public class AssignTicketCommandHandler : ICommandHandler<AssignTicketCommand, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDetailDto> HandleAsync(AssignTicketCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        ticket.AssignTo(command.AssignedToUserId, command.AssignedByName);

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }
}

public class ChangeTicketPriorityCommandHandler : ICommandHandler<ChangeTicketPriorityCommand, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeTicketPriorityCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDetailDto> HandleAsync(ChangeTicketPriorityCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        ticket.ChangePriority(command.Priority, null, command.UpdatedByName);

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }
}

public class ChangeTicketStatusCommandHandler : ICommandHandler<ChangeTicketStatusCommand, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeTicketStatusCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDetailDto> HandleAsync(ChangeTicketStatusCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        ticket.ChangeStatus(command.Status, null, command.UpdatedByName);

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }
}

public class ResolveTicketCommandHandler : ICommandHandler<ResolveTicketCommand, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDetailDto> HandleAsync(ResolveTicketCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        ticket.Resolve(command.Resolution, command.ResolvedAt, null, command.ResolvedByName);

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }
}

public class CloseTicketCommandHandler : ICommandHandler<CloseTicketCommand, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDetailDto> HandleAsync(CloseTicketCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        ticket.Close(null, command.ClosedByName);

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }
}

public class CancelTicketCommandHandler : ICommandHandler<CancelTicketCommand, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDetailDto> HandleAsync(CancelTicketCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        ticket.Cancel(command.Reason, null, command.CancelledByName);

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }
}

public class ReopenTicketCommandHandler : ICommandHandler<ReopenTicketCommand, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReopenTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketDetailDto> HandleAsync(ReopenTicketCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        ticket.Reopen(command.Reason, null, command.ReopenedByName);

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ticket.ToDetailDto();
    }
}

public class DeleteTicketCommandHandler : ICommandHandler<DeleteTicketCommand, bool>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteTicketCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        ticket.SoftDelete();
        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public class AddTicketCommentCommandHandler : ICommandHandler<AddTicketCommentCommand, TicketCommentDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddTicketCommentCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketCommentDto> HandleAsync(AddTicketCommentCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(command.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", command.TicketId);

        var comment = ticket.AddComment(command.AuthorUserId, command.AuthorName, command.Message);

        _ticketRepository.Update(ticket);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return comment.ToDto();
    }
}
