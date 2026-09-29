using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Leads.Commands;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Domain.ValueObjects;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class LeadCommandHandlerTests
{
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();
    private readonly ICompanyRepository _companyRepository = Substitute.For<ICompanyRepository>();
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateLeadHandler_WithValidCompany_ShouldCreateAndReturnLeadDto()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = Company.Create("Acme Ltd", "Tech", "", "", "", new Address("Street 1"));
        _companyRepository.GetByIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(company);

        var handler = new CreateLeadCommandHandler(_leadRepository, _companyRepository, _contactRepository, _unitOfWork);
        var command = new CreateLeadCommand(companyId, "Web App", "Referral", "Project description", 25000m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Web App", result.Title);
        Assert.Equal("Referral", result.Source);
        Assert.Equal(25000m, result.EstimatedValue);
        Assert.Equal("NEW", result.Status);
        await _leadRepository.Received(1).AddAsync(Arg.Any<Lead>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLeadHandler_WithMissingCompany_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        _companyRepository.GetByIdAsync(companyId, Arg.Any<CancellationToken>()).Returns((Company?)null);

        var handler = new CreateLeadCommandHandler(_leadRepository, _companyRepository, _contactRepository, _unitOfWork);
        var command = new CreateLeadCommand(companyId, "Web App", "Referral");

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task QualifyLeadHandler_WithExistingLead_ShouldQualifyAndSave()
    {
        // Arrange
        var lead = Lead.Create(Guid.NewGuid(), "Portal Dev", "Inbound");
        _leadRepository.GetByIdAsync(lead.Id, Arg.Any<CancellationToken>()).Returns(lead);

        var handler = new QualifyLeadCommandHandler(_leadRepository, _unitOfWork);
        var command = new QualifyLeadCommand(lead.Id);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal("QUALIFIED", result.Status);
        _leadRepository.Received(1).Update(lead);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DisqualifyLeadHandler_WithReason_ShouldDisqualifyAndSave()
    {
        // Arrange
        var lead = Lead.Create(Guid.NewGuid(), "Mobile App", "Outbound");
        _leadRepository.GetByIdAsync(lead.Id, Arg.Any<CancellationToken>()).Returns(lead);

        var handler = new DisqualifyLeadCommandHandler(_leadRepository, _unitOfWork);
        var command = new DisqualifyLeadCommand(lead.Id, "Too expensive for client");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal("DISQUALIFIED", result.Status);
        Assert.Equal("Too expensive for client", result.DisqualificationReason);
        _leadRepository.Received(1).Update(lead);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignLeadHandler_WithValidUser_ShouldAssignAndSave()
    {
        // Arrange
        var lead = Lead.Create(Guid.NewGuid(), "Cloud Migration", "Web");
        var userId = Guid.NewGuid();
        _leadRepository.GetByIdAsync(lead.Id, Arg.Any<CancellationToken>()).Returns(lead);

        var handler = new AssignLeadCommandHandler(_leadRepository, _unitOfWork);
        var command = new AssignLeadCommand(lead.Id, userId);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(userId, result.AssignedTo);
        _leadRepository.Received(1).Update(lead);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveLeadHandler_ShouldSoftDeleteAndSave()
    {
        // Arrange
        var lead = Lead.Create(Guid.NewGuid(), "Obsolete Lead", "Web");
        _leadRepository.GetByIdAsync(lead.Id, Arg.Any<CancellationToken>()).Returns(lead);

        var handler = new ArchiveLeadCommandHandler(_leadRepository, _unitOfWork);
        var command = new ArchiveLeadCommand(lead.Id, Guid.NewGuid());

        // Act
        await handler.HandleAsync(command);

        // Assert
        Assert.True(lead.IsDeleted);
        _leadRepository.Received(1).Update(lead);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
