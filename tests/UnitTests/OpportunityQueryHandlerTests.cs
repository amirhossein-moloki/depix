using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Sales.Application.Features.Opportunities.Queries;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class OpportunityQueryHandlerTests
{
    private readonly IOpportunityRepository _repository;

    public OpportunityQueryHandlerTests()
    {
        _repository = Substitute.For<IOpportunityRepository>();
    }

    [Fact]
    public async Task GetById_ExistingOpportunity_ShouldReturnDto()
    {
        // Arrange
        var opportunity = Opportunity.Create("Website Deal", value: Money.Create(8000, "USD"));
        _repository.GetByIdAsync(opportunity.Id, Arg.Any<CancellationToken>()).Returns(opportunity);

        var query = new GetOpportunityByIdQuery(opportunity.Id);
        var handler = new GetOpportunityByIdQueryHandler(_repository);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(opportunity.Id, result.Id);
        Assert.Equal("Website Deal", result.Title);
        Assert.Equal(8000, result.ValueAmount);
    }

    [Fact]
    public async Task GetById_NonExistentOpportunity_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Opportunity?)null);

        var query = new GetOpportunityByIdQuery(id);
        var handler = new GetOpportunityByIdQueryHandler(_repository);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(query));
    }

    [Fact]
    public async Task GetOpportunityPipeline_ShouldReturnSummariesByStage()
    {
        // Arrange
        var opp1 = Opportunity.Create("Deal 1", stage: OpportunityStage.New, value: Money.Create(5000, "USD"));
        var opp2 = Opportunity.Create("Deal 2", stage: OpportunityStage.New, value: Money.Create(10000, "USD"));
        var opp3 = Opportunity.Create("Deal 3", stage: OpportunityStage.Proposal, value: Money.Create(20000, "USD"));

        var summaries = new List<OpportunityPipelineStageSummary>
        {
            new(OpportunityStage.New, 2, 15000, new List<Opportunity> { opp1, opp2 }),
            new(OpportunityStage.Proposal, 1, 20000, new List<Opportunity> { opp3 }),
            new(OpportunityStage.Won, 0, 0, new List<Opportunity>())
        };

        _repository.GetPipelineSummaryAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(summaries);

        var query = new GetOpportunityPipelineQuery();
        var handler = new GetOpportunityPipelineQueryHandler(_repository);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(35000, result.TotalPipelineValue);

        var newStage = result.Stages.FirstOrDefault(s => s.Stage == OpportunityStage.New);
        Assert.NotNull(newStage);
        Assert.Equal(2, newStage.Count);
        Assert.Equal(15000, newStage.TotalValue);
    }
}
