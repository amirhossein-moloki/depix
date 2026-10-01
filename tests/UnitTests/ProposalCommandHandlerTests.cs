using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Moq;
using Modules.Sales.Application.Features.Proposals.Commands;
using Modules.Sales.Application.Features.Proposals.Queries;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Entities;
using Modules.Sales.Domain.Repositories;
using Xunit;

namespace UnitTests;

public class ProposalCommandHandlerTests
{
    private readonly Mock<IOpportunityRepository> _opportunityRepositoryMock = new();
    private readonly Mock<IProposalRepository> _proposalRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_CreateProposalCommand_WhenOpportunityExists_ShouldCreateProposal()
    {
        var opportunityId = Guid.NewGuid();
        var opportunity = Opportunity.Create("Test Opp", leadId: Guid.NewGuid(), value: Money.Create(5000m, "USD"));

        _opportunityRepositoryMock.Setup(r => r.GetByIdAsync(opportunityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(opportunity);

        var handler = new CreateProposalCommandHandler(
            _opportunityRepositoryMock.Object,
            _proposalRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateProposalCommand(
            opportunityId,
            "Proposal Title",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)),
            Version: "1.0",
            Currency: "USD"
        );

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal("Proposal Title", result.Title);
        Assert.Equal(opportunityId, result.OpportunityId);
        _proposalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Proposal>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreateProposalCommand_WhenOpportunityNotFound_ShouldThrowEntityNotFoundException()
    {
        var opportunityId = Guid.NewGuid();
        _opportunityRepositoryMock.Setup(r => r.GetByIdAsync(opportunityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Opportunity?)null);

        var handler = new CreateProposalCommandHandler(
            _opportunityRepositoryMock.Object,
            _proposalRepositoryMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateProposalCommand(opportunityId, "Title", DateOnly.FromDateTime(DateTime.UtcNow));

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task Handle_AddProposalItemCommand_WhenProposalExists_ShouldAddItemAndRecalculate()
    {
        var proposalId = Guid.NewGuid();
        var proposal = Proposal.Create(Guid.NewGuid(), "Proposal Title", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));

        _proposalRepositoryMock.Setup(r => r.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);

        var handler = new AddProposalItemCommandHandler(_proposalRepositoryMock.Object, _unitOfWorkMock.Object);
        var command = new AddProposalItemCommand(proposalId, "Web Hosting", "Annual hosting", 1, 200m, 20m);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Single(result.ProposalItems);
        Assert.Equal(180m, result.Total);
        _proposalRepositoryMock.Verify(r => r.Update(proposal), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ChangeProposalStatusCommand_WithValidStatus_ShouldUpdateStatus()
    {
        var proposalId = Guid.NewGuid();
        var proposal = Proposal.Create(Guid.NewGuid(), "Proposal Title", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));

        _proposalRepositoryMock.Setup(r => r.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);

        var handler = new ChangeProposalStatusCommandHandler(_proposalRepositoryMock.Object, _unitOfWorkMock.Object);
        var command = new ChangeProposalStatusCommand(proposalId, ProposalStatus.Accepted);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal(ProposalStatus.Accepted, result.Status);
        _proposalRepositoryMock.Verify(r => r.Update(proposal), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class ProposalQueryHandlerTests
{
    private readonly Mock<IProposalRepository> _proposalRepositoryMock = new();

    [Fact]
    public async Task Handle_GetProposalByIdQuery_WhenExists_ShouldReturnDto()
    {
        var proposalId = Guid.NewGuid();
        var proposal = Proposal.Create(Guid.NewGuid(), "Proposal Title", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));

        _proposalRepositoryMock.Setup(r => r.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(proposal);

        var handler = new GetProposalByIdQueryHandler(_proposalRepositoryMock.Object);
        var query = new GetProposalByIdQuery(proposalId);

        var result = await handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal("Proposal Title", result.Title);
    }

    [Fact]
    public async Task Handle_GetProposalByIdQuery_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        var proposalId = Guid.NewGuid();
        _proposalRepositoryMock.Setup(r => r.GetByIdAsync(proposalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Proposal?)null);

        var handler = new GetProposalByIdQueryHandler(_proposalRepositoryMock.Object);
        var query = new GetProposalByIdQuery(proposalId);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(query));
    }
}
