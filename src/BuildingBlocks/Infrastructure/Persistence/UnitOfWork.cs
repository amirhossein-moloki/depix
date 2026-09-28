using BuildingBlocks.Application.Events;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IEventBus _eventBus;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(ApplicationDbContext dbContext, IEventBus eventBus)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var aggregateRoots = _dbContext.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregateRoots)
        {
            aggregate.ClearDomainEvents();
        }

        var result = await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _eventBus.PublishAsync((dynamic)domainEvent, cancellationToken);
        }

        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null) return;
        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }
}
