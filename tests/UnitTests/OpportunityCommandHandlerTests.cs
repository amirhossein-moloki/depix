using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Sales.Application.Features.Opportunities.Commands;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class OpportunityCommandHandlerTests
{
    private readonly IOpportunityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public OpportunityCommandHandlerTests()
    {
        _repository = Substitute.For<IOpportunityRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
    }

    [Fact]
    public async Task CreateOpportunity_ValidCommand_ShouldAddAndSave()
    {
        // Arrange
        var command = new CreateOpportunityCommand(
            Title: "Custom Web App",
            Description: "React + .NET 8 backend",
            ValueAmount: 25000,
            ValueCurrency: "USD",
            Stage: OpportunityStage.Qualified
        );

        var handler = new CreateOpportunityCommandHandler(_repository, _unitOfWork);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Custom Web App", result.Title);
        Assert.Equal(25000, result.ValueAmount);
        Assert.Equal("USD", result.ValueCurrency);
        Assert.Equal(OpportunityStage.Qualified, result.Stage);

        await _repository.Received(1).AddAsync(Arg.Any<Opportunity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateOpportunity_ExistingEntity_ShouldUpdateDetailsAndSave()
    {
        // Arrange
        var opportunity = Opportunity.Create("Initial Title", value: Money.Create(10000, "USD"));
        _repository.GetByIdAsync(opportunity.Id, Arg.Any<CancellationToken>()).Returns(opportunity);

        var command = new UpdateOpportunityCommand(
            Id: opportunity.Id,
            Title: "Updated Title",
            Description: "Updated Description",
            ValueAmount: 18000,
            ValueCurrency: "USD",
            Probability: 60,
            ExpectedCloseDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15))
        );

        var handler = new UpdateOpportunityCommandHandler(_repository, _unitOfWork);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(18000, result.ValueAmount);
        Assert.Equal(60, result.Probability);

        _repository.Received(1).Update(opportunity);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignOpportunity_ExistingEntity_ShouldAssignToUser()
    {
        // Arrange
        var opportunity = Opportunity.Create("Site Integration");
        var userId = Guid.NewGuid();
        _repository.GetByIdAsync(opportunity.Id, Arg.Any<CancellationToken>()).Returns(opportunity);

        var command = new AssignOpportunityCommand(opportunity.Id, userId);
        var handler = new AssignOpportunityCommandHandler(_repository, _unitOfWork);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(userId, result.AssignedTo);
        _repository.Received(1).Update(opportunity);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarkOpportunityWon_ExistingEntity_ShouldMarkWonAndSave()
    {
        // Arrange
        var opportunity = Opportunity.Create("Redesign Portal", value: Money.Create(12000, "USD"));
        _repository.GetByIdAsync(opportunity.Id, Arg.Any<CancellationToken>()).Returns(opportunity);

        var command = new MarkOpportunityWonCommand(opportunity.Id);
        var handler = new MarkOpportunityWonCommandHandler(_repository, _unitOfWork);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(OpportunityStage.Won, result.Stage);
        Assert.Equal(OpportunityStatus.Won, result.Status);
        Assert.Equal(100, result.Probability);
        Assert.NotNull(result.WonAt);

        _repository.Received(1).Update(opportunity);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarkOpportunityLost_ExistingEntity_ShouldMarkLostAndSave()
    {
        // Arrange
        var opportunity = Opportunity.Create("SEO Package");
        _repository.GetByIdAsync(opportunity.Id, Arg.Any<CancellationToken>()).Returns(opportunity);

        var command = new MarkOpportunityLostCommand(opportunity.Id, "Client selected another vendor");
        var handler = new MarkOpportunityLostCommandHandler(_repository, _unitOfWork);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(OpportunityStage.Lost, result.Stage);
        Assert.Equal(OpportunityStatus.Lost, result.Status);
        Assert.Equal("Client selected another vendor", result.LossReason);
        Assert.NotNull(result.LostAt);

        _repository.Received(1).Update(opportunity);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Command_NonExistentOpportunity_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repository.GetByIdAsync(nonExistentId, Arg.Any<CancellationToken>()).Returns((Opportunity?)null);

        var handler = new MarkOpportunityWonCommandHandler(_repository, _unitOfWork);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(new MarkOpportunityWonCommand(nonExistentId)));
    }
}
