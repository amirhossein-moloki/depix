using BuildingBlocks.Application.Events;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Moq;
using Xunit;

namespace UnitTests;

public class UnitOfWorkTests
{
    static UnitOfWorkTests()
    {
        _ = Modules.CRM.Infrastructure.AssemblyReference.Assembly;
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchDomainEventsAndClearQueue()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new ApplicationDbContext(options);
        var mockEventBus = new Mock<IEventBus>();

        var unitOfWork = new UnitOfWork(dbContext, mockEventBus.Object);

        var lead = Lead.Create(Guid.NewGuid(), "Inbound");
        lead.Qualify();
        lead.ConvertToCustomer(); // raises 4 events: LeadStatusChangedEvent for Qualify, LeadQualifiedEvent, LeadStatusChangedEvent for Convert, and LeadConvertedEvent

        dbContext.Set<Lead>().Add(lead);

        // Act
        var result = await unitOfWork.SaveChangesAsync();

        // Assert
        Assert.True(result > 0);
        Assert.Empty(lead.DomainEvents);
        mockEventBus.Verify(eb => eb.PublishAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
    }
}
