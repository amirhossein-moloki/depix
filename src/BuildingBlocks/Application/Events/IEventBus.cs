using BuildingBlocks.Domain.Events;

namespace BuildingBlocks.Application.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class, IDomainEvent;
}
