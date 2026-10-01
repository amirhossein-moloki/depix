using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Support.Application.DTOs;
using Modules.Support.Application.Mappings;
using Modules.Support.Domain.Repositories;

namespace Modules.Support.Application.Queries;

public record GetTicketByIdQuery(Guid TicketId) : IQuery<TicketDetailDto>;

public record GetTicketsQuery(TicketFilterParams FilterParams) : IQuery<PagedResult<TicketListItemDto>>;

public record GetCustomerTicketsQuery(Guid CustomerId) : IQuery<List<TicketListItemDto>>;

public record GetProjectTicketsQuery(Guid ProjectId) : IQuery<List<TicketListItemDto>>;

public record GetTicketCommentsQuery(Guid TicketId) : IQuery<List<TicketCommentDto>>;

public class GetTicketByIdQueryHandler : IQueryHandler<GetTicketByIdQuery, TicketDetailDto>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketDetailDto> HandleAsync(GetTicketByIdQuery query, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(query.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", query.TicketId);

        return ticket.ToDetailDto();
    }
}

public class GetTicketsQueryHandler : IQueryHandler<GetTicketsQuery, PagedResult<TicketListItemDto>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<PagedResult<TicketListItemDto>> HandleAsync(GetTicketsQuery query, CancellationToken cancellationToken = default)
    {
        var filter = query.FilterParams ?? new TicketFilterParams();
        var tickets = await _ticketRepository.GetTicketsAsync(filter, cancellationToken);
        var totalCount = await _ticketRepository.GetCountAsync(filter, cancellationToken);

        var dtos = tickets.Select(t => t.ToListItemDto()).ToList();
        return new PagedResult<TicketListItemDto>(dtos, totalCount, filter.PageNumber, filter.PageSize);
    }
}

public class GetCustomerTicketsQueryHandler : IQueryHandler<GetCustomerTicketsQuery, List<TicketListItemDto>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetCustomerTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<List<TicketListItemDto>> HandleAsync(GetCustomerTicketsQuery query, CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);
        return tickets.Select(t => t.ToListItemDto()).ToList();
    }
}

public class GetProjectTicketsQueryHandler : IQueryHandler<GetProjectTicketsQuery, List<TicketListItemDto>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetProjectTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<List<TicketListItemDto>> HandleAsync(GetProjectTicketsQuery query, CancellationToken cancellationToken = default)
    {
        var tickets = await _ticketRepository.GetByProjectIdAsync(query.ProjectId, cancellationToken);
        return tickets.Select(t => t.ToListItemDto()).ToList();
    }
}

public class GetTicketCommentsQueryHandler : IQueryHandler<GetTicketCommentsQuery, List<TicketCommentDto>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketCommentsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<List<TicketCommentDto>> HandleAsync(GetTicketCommentsQuery query, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(query.TicketId, cancellationToken)
            ?? throw new EntityNotFoundException("Ticket", query.TicketId);

        return ticket.Comments.Select(c => c.ToDto()).ToList();
    }
}
