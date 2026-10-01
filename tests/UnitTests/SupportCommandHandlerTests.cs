using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Support.Application.Commands;
using Modules.Support.Application.Services;
using Modules.Support.Domain.Constants;
using Modules.Support.Domain.Entities;
using Modules.Support.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class SupportCommandHandlerTests
{
    private readonly ITicketRepository _ticketRepository = Substitute.For<ITicketRepository>();
    private readonly ITicketNumberGenerator _numberGenerator = Substitute.For<ITicketNumberGenerator>();
    private readonly ICustomerService _customerService = Substitute.For<ICustomerService>();
    private readonly IProjectService _projectService = Substitute.For<IProjectService>();
    private readonly ICustomerContactService _contactService = Substitute.For<ICustomerContactService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateTicket_WhenCustomerExists_ShouldCreateAndReturnTicketDto()
    {
        var customerId = Guid.NewGuid();
        _customerService.CustomerExistsAsync(customerId, Arg.Any<CancellationToken>()).Returns(true);
        _numberGenerator.GenerateTicketNumberAsync(Arg.Any<CancellationToken>()).Returns("TICK-202610-0001");

        var handler = new CreateTicketCommandHandler(
            _ticketRepository,
            _numberGenerator,
            _customerService,
            _projectService,
            _contactService,
            _unitOfWork);

        var command = new CreateTicketCommand(
            customerId,
            null,
            null,
            "Broken Portal",
            "Login fails with 500 error",
            TicketPriority.High,
            TicketCategory.Bug,
            null,
            null);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal("TICK-202610-0001", result.TicketNumber);
        Assert.Equal("Broken Portal", result.Subject);
        Assert.Equal(TicketStatus.Open, result.Status);

        await _ticketRepository.Received(1).AddAsync(Arg.Any<Ticket>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateTicket_WhenCustomerDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        var customerId = Guid.NewGuid();
        _customerService.CustomerExistsAsync(customerId, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new CreateTicketCommandHandler(
            _ticketRepository,
            _numberGenerator,
            _customerService,
            _projectService,
            _contactService,
            _unitOfWork);

        var command = new CreateTicketCommand(
            customerId,
            null,
            null,
            "Broken Portal",
            "Login fails",
            TicketPriority.Normal,
            TicketCategory.General,
            null,
            null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task CreateTicket_WhenProjectDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        var customerId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        _customerService.CustomerExistsAsync(customerId, Arg.Any<CancellationToken>()).Returns(true);
        _projectService.ProjectExistsAsync(projectId, Arg.Any<CancellationToken>()).Returns(false);

        var handler = new CreateTicketCommandHandler(
            _ticketRepository,
            _numberGenerator,
            _customerService,
            _projectService,
            _contactService,
            _unitOfWork);

        var command = new CreateTicketCommand(
            customerId,
            projectId,
            null,
            "Broken Portal",
            "Login fails",
            TicketPriority.Normal,
            TicketCategory.General,
            null,
            null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task AssignTicket_WhenTicketExists_ShouldAssignUser()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        _ticketRepository.GetByIdAsync(ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);

        var handler = new AssignTicketCommandHandler(_ticketRepository, _unitOfWork);
        var assignUser = Guid.NewGuid();

        var result = await handler.HandleAsync(new AssignTicketCommand(ticket.Id, assignUser, "Manager"));

        Assert.Equal(assignUser, result.AssignedToUserId);
        _ticketRepository.Received(1).Update(ticket);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveTicket_WhenTicketExists_ShouldUpdateResolutionAndStatus()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        _ticketRepository.GetByIdAsync(ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);

        var handler = new ResolveTicketCommandHandler(_ticketRepository, _unitOfWork);

        var result = await handler.HandleAsync(new ResolveTicketCommand(ticket.Id, "Applied patch v1.2", null, "Dev"));

        Assert.Equal(TicketStatus.Resolved, result.Status);
        Assert.Equal("Applied patch v1.2", result.Resolution);
        Assert.NotNull(result.ResolvedAt);
        _ticketRepository.Received(1).Update(ticket);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddTicketComment_WhenTicketExists_ShouldAddCommentAndReturnDto()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Subject", "Description");
        _ticketRepository.GetByIdAsync(ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);

        var handler = new AddTicketCommentCommandHandler(_ticketRepository, _unitOfWork);
        var authorId = Guid.NewGuid();

        var result = await handler.HandleAsync(new AddTicketCommentCommand(ticket.Id, authorId, "Agent Smith", "Investigating logs"));

        Assert.NotNull(result);
        Assert.Equal("Investigating logs", result.Message);
        Assert.Equal("Agent Smith", result.AuthorName);
        _ticketRepository.Received(1).Update(ticket);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
