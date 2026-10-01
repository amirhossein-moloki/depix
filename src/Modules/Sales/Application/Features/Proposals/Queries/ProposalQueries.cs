using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Proposals.DTOs;
using Modules.Sales.Application.Features.Proposals.Mappings;
using Modules.Sales.Domain.Repositories;

namespace Modules.Sales.Application.Features.Proposals.Queries;

public record GetProposalByIdQuery(Guid Id) : IQuery<ProposalDto>;

public class GetProposalByIdQueryHandler : IQueryHandler<GetProposalByIdQuery, ProposalDto>
{
    private readonly IProposalRepository _proposalRepository;

    public GetProposalByIdQueryHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<ProposalDto> HandleAsync(GetProposalByIdQuery query, CancellationToken cancellationToken = default)
    {
        var proposal = await _proposalRepository.GetByIdAsync(query.Id, cancellationToken);
        if (proposal == null || proposal.IsDeleted)
        {
            throw new EntityNotFoundException("Proposal", query.Id);
        }

        return proposal.ToDto();
    }
}

public record GetProposalsQuery(
    int Page = 1,
    int PageSize = 10,
    Guid? OpportunityId = null,
    Guid? CustomerId = null,
    Guid? CompanyId = null,
    string? Status = null
) : IQuery<PagedResult<ProposalListItemDto>>;

public class GetProposalsQueryHandler : IQueryHandler<GetProposalsQuery, PagedResult<ProposalListItemDto>>
{
    private readonly IProposalRepository _proposalRepository;

    public GetProposalsQueryHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<PagedResult<ProposalListItemDto>> HandleAsync(GetProposalsQuery query, CancellationToken cancellationToken = default)
    {
        var filter = new ProposalFilterParams(
            query.Page,
            query.PageSize,
            query.OpportunityId,
            query.CustomerId,
            query.CompanyId,
            query.Status
        );

        var (items, totalCount) = await _proposalRepository.GetFilteredAsync(filter, cancellationToken);
        var dtos = items.Select(p => p.ToListItemDto()).ToList();

        return new PagedResult<ProposalListItemDto>(dtos, totalCount, query.Page, query.PageSize);
    }
}

public record GetOpportunityProposalsQuery(Guid OpportunityId) : IQuery<List<ProposalListItemDto>>;

public class GetOpportunityProposalsQueryHandler : IQueryHandler<GetOpportunityProposalsQuery, List<ProposalListItemDto>>
{
    private readonly IProposalRepository _proposalRepository;

    public GetOpportunityProposalsQueryHandler(IProposalRepository proposalRepository)
    {
        _proposalRepository = proposalRepository;
    }

    public async Task<List<ProposalListItemDto>> HandleAsync(GetOpportunityProposalsQuery query, CancellationToken cancellationToken = default)
    {
        var proposals = await _proposalRepository.GetByOpportunityIdAsync(query.OpportunityId, cancellationToken);
        return proposals.Select(p => p.ToListItemDto()).ToList();
    }
}
