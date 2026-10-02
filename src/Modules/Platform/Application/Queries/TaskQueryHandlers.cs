using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Application.Mappings;
using Modules.Platform.Domain.Repositories;

namespace Modules.Platform.Application.Queries;

public class GetTaskByIdQueryHandler : IQueryHandler<GetTaskByIdQuery, WorkTaskDto>
{
    private readonly IWorkTaskRepository _repository;

    public GetTaskByIdQueryHandler(IWorkTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkTaskDto> HandleAsync(GetTaskByIdQuery query, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(query.Id, cancellationToken);
        if (task == null)
        {
            throw new EntityNotFoundException("WorkTask", query.Id);
        }

        return task.ToDto();
    }
}

public class GetTasksQueryHandler : IQueryHandler<GetTasksQuery, List<WorkTaskDto>>
{
    private readonly IWorkTaskRepository _repository;

    public GetTasksQueryHandler(IWorkTaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WorkTaskDto>> HandleAsync(GetTasksQuery query, CancellationToken cancellationToken = default)
    {
        var tasks = await _repository.GetListAsync(query.AssignedTo, query.Status, cancellationToken);
        return tasks.Select(t => t.ToDto()).ToList();
    }
}
