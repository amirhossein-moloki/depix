using BuildingBlocks.Common.Exceptions;
using Modules.Support.Application.DTOs;
using Modules.Support.Application.Queries;
using Modules.Support.Domain.Entities;
using Modules.Support.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class SupportQueryHandlerTests
{
    private readonly ITicketRepository _ticketRepository = Substitute.For<ITicketRepository>();

    [Fact]
    public async Task GetTicketById_WhenFound_ShouldReturnDetailDto()
    {
        var ticket = Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Bug in Portal", "Error 500");
        _ticketRepository.GetByIdAsync(ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);

        var handler = new GetTicketByIdQueryHandler(_ticketRepository);

        var result = await handler.HandleAsync(new GetTicketByIdQuery(ticket.Id));

        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal("Bug in Portal", result.Subject);
    }

    [Fact]
    public async Task GetTicketById_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var ticketId = Guid.NewGuid();
        _ticketRepository.GetByIdAsync(ticketId, Arg.Any<CancellationToken>()).Returns((Ticket?)null);

        var handler = new GetTicketByIdQueryHandler(_ticketRepository);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(new GetTicketByIdQuery(ticketId)));
    }

    [Fact]
    public async Task GetTickets_ShouldReturnPagedResult()
    {
        var tickets = new List<Ticket>
        {
            Ticket.Create("TICK-0001", Guid.NewGuid(), null, null, "Ticket 1", "Desc 1"),
            Ticket.Create("TICK-0002", Guid.NewGuid(), null, null, "Ticket 2", "Desc 2")
        };

        _ticketRepository.GetTicketsAsync(Arg.Any<TicketFilterParams>(), Arg.Any<CancellationToken>()).Returns(tickets);
        _ticketRepository.GetCountAsync(Arg.Any<TicketFilterParams>(), Arg.Any<CancellationToken>()).Returns(2);

        var handler = new GetTicketsQueryHandler(_ticketRepository);

        var result = await handler.HandleAsync(new GetTicketsQuery(new TicketFilterParams(PageNumber: 1, PageSize: 10)));

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetCustomerTickets_ShouldReturnListForCustomer()
    {
        var customerId = Guid.NewGuid();
        var tickets = new List<Ticket>
        {
            Ticket.Create("TICK-0001", customerId, null, null, "Customer Ticket", "Desc")
        };

        _ticketRepository.GetByCustomerIdAsync(customerId, Arg.Any<CancellationToken>()).Returns(tickets);

        var handler = new GetCustomerTicketsQueryHandler(_ticketRepository);

        var result = await handler.HandleAsync(new GetCustomerTicketsQuery(customerId));

        Assert.Single(result);
        Assert.Equal(customerId, result[0].CustomerId);
    }

    [Fact]
    public async Task GetProjectTickets_ShouldReturnListForProject()
    {
        var projectId = Guid.NewGuid();
        var tickets = new List<Ticket>
        {
            Ticket.Create("TICK-0001", Guid.NewGuid(), projectId, null, "Project Ticket", "Desc")
        };

        _ticketRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>()).Returns(tickets);

        var handler = new GetProjectTicketsQueryHandler(_ticketRepository);

        var result = await handler.HandleAsync(new GetProjectTicketsQuery(projectId));

        Assert.Single(result);
        Assert.Equal(projectId, result[0].ProjectId);
    }
}
