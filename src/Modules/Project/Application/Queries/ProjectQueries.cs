using BuildingBlocks.Application.CQRS;
using Modules.Project.Application.DTOs;
using Modules.Project.Application.Mappings;
using Modules.Project.Domain.Repositories;

namespace Modules.Project.Application.Queries;

public record GetProjectByIdQuery(Guid Id) : IQuery<ProjectDetailDto>;

public record GetProjectsQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string? Status = null,
    Guid? CustomerId = null,
    Guid? CompanyId = null,
    Guid? OpportunityId = null,
    Guid? ProposalId = null,
    DateOnly? StartDateFrom = null,
    DateOnly? StartDateTo = null,
    DateOnly? PlannedDeliveryDateFrom = null,
    DateOnly? PlannedDeliveryDateTo = null,
    string? SortBy = null,
    bool SortDescending = false) : IQuery<PagedResult<ProjectListItemDto>>;

public class GetProjectByIdQueryHandler : IQueryHandler<GetProjectByIdQuery, ProjectDetailDto>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDetailDto> HandleAsync(GetProjectByIdQuery query, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(query.Id, cancellationToken);
        if (project == null || project.IsDeleted)
        {
            throw new KeyNotFoundException($"Project with ID '{query.Id}' was not found.");
        }

        return project.ToDetailDto();
    }
}

public class GetProjectsQueryHandler : IQueryHandler<GetProjectsQuery, PagedResult<ProjectListItemDto>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<PagedResult<ProjectListItemDto>> HandleAsync(GetProjectsQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _projectRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Search,
            query.Status,
            query.CustomerId,
            query.CompanyId,
            query.OpportunityId,
            query.ProposalId,
            query.StartDateFrom,
            query.StartDateTo,
            query.PlannedDeliveryDateFrom,
            query.PlannedDeliveryDateTo,
            query.SortBy,
            query.SortDescending,
            cancellationToken);

        var listDtos = items.Select(p => p.ToListItemDto()).ToList();
        return new PagedResult<ProjectListItemDto>(listDtos, totalCount, query.Page, query.PageSize);
    }
}
