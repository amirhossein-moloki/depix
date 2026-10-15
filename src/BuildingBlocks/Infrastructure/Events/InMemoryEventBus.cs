using BuildingBlocks.Application.Events;
using BuildingBlocks.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Events;

public class InMemoryEventBus : IEventBus
{
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(ILogger<InMemoryEventBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IDomainEvent
    {
        _logger.LogInformation("Domain event published: {EventType} {@Event}", typeof(TEvent).Name, @event);
        return Task.CompletedTask;
    }
}
