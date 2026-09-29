using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Leads.Queries;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class LeadQueryHandlerTests
{
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();

    [Fact]
    public async Task GetLeadByIdHandler_WithExistingId_ShouldReturnLeadDto()
    {
        // Arrange
        var lead = Lead.Create(Guid.NewGuid(), "CRM System", "Referral", "Full CRM suite", 100000m);
        _leadRepository.GetByIdAsync(lead.Id, Arg.Any<CancellationToken>()).Returns(lead);

        var handler = new GetLeadByIdQueryHandler(_leadRepository);
        var query = new GetLeadByIdQuery(lead.Id);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(lead.Id, result.Id);
        Assert.Equal("CRM System", result.Title);
        Assert.Equal(100000m, result.EstimatedValue);
    }

    [Fact]
    public async Task GetLeadByIdHandler_WithMissingId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _leadRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Lead?)null);

        var handler = new GetLeadByIdQueryHandler(_leadRepository);
        var query = new GetLeadByIdQuery(id);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(query));
    }

    [Fact]
    public async Task GetLeadsQueryHandler_ShouldReturnPagedResult()
    {
        // Arrange
        var l1 = Lead.Create(Guid.NewGuid(), "Lead 1", "Web");
        var l2 = Lead.Create(Guid.NewGuid(), "Lead 2", "Referral");
        var list = new List<Lead> { l1, l2 };

        _leadRepository.GetListAsync(
            1, 10, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<string?>(), Arg.Any<bool>(),
            Arg.Any<CancellationToken>()).Returns(list);

        _leadRepository.CountAsync(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(),
            Arg.Any<string?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>()).Returns(2);

        var handler = new GetLeadsQueryHandler(_leadRepository);
        var query = new GetLeadsQuery(1, 10);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetLeadPipelineQueryHandler_ShouldGroupLeadsByStages()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var l1 = Lead.Create(companyId, "New Lead", "Web", estimatedValue: 10000m, status: "NEW");
        var l2 = Lead.Create(companyId, "Qualified Lead", "Referral", estimatedValue: 20000m, status: "NEW");
        l2.Qualify(); // Set to QUALIFIED

        var list = new List<Lead> { l1, l2 };

        _leadRepository.GetListAsync(
            1, 1000, null, null, companyId, null, null, null, null, "CreatedAt", true, Arg.Any<CancellationToken>())
            .Returns(list);

        var handler = new GetLeadPipelineQueryHandler(_leadRepository);
        var query = new GetLeadPipelineQuery(companyId);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalLeadsCount);
        Assert.Equal(30000m, result.TotalPipelineValue);
        Assert.Contains(result.Stages, s => s.Status == "NEW" && s.Count == 1);
        Assert.Contains(result.Stages, s => s.Status == "QUALIFIED" && s.Count == 1);
    }
}
