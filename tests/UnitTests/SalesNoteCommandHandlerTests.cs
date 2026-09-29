using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.SalesNotes.Commands;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Events;
using Modules.CRM.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class SalesNoteCommandHandlerTests
{
    private readonly ISalesNoteRepository _salesNoteRepository = Substitute.For<ISalesNoteRepository>();
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateSalesNoteCommandHandler_WithValidData_ShouldCreateNoteAndAddDomainEvent()
    {
        var lead = Lead.Create(Guid.NewGuid(), "CRM Upgrade", "Inbound");
        _leadRepository.GetByIdAsync(lead.Id, Arg.Any<CancellationToken>()).Returns(lead);

        var handler = new CreateSalesNoteCommandHandler(_salesNoteRepository, _leadRepository, _contactRepository, _unitOfWork);
        var command = new CreateSalesNoteCommand(
            lead.Id,
            "Tech Requirements",
            "Needs REST APIs and PostgreSQL",
            "None",
            "Modular architecture pitch",
            80,
            Guid.NewGuid(),
            null,
            null,
            "Competitor Y",
            "$100k",
            "VP of Engineering"
        );

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(lead.Id, result.LeadId);
        Assert.Equal("Tech Requirements", result.Title);
        Assert.Equal("Needs REST APIs and PostgreSQL", result.NeedAnalysis);
        Assert.Equal(80, result.Probability);

        await _salesNoteRepository.Received(1).AddAsync(Arg.Any<SalesNote>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Contains(lead.DomainEvents, e => e is SalesNoteCreatedEvent);
    }

    [Fact]
    public async Task CreateSalesNoteCommandHandler_WithNonExistentLead_ShouldThrowEntityNotFoundException()
    {
        _leadRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Lead?)null);

        var handler = new CreateSalesNoteCommandHandler(_salesNoteRepository, _leadRepository, _contactRepository, _unitOfWork);
        var command = new CreateSalesNoteCommand(Guid.NewGuid(), "Title", "Needs", "Obj", "Strat", 50, Guid.NewGuid());

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateSalesNoteCommandHandler_WithValidData_ShouldUpdateNote()
    {
        var note = SalesNote.Create(Guid.NewGuid(), "Title", "Need A", "Obj A", "Strat A", 30, Guid.NewGuid());
        _salesNoteRepository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var handler = new UpdateSalesNoteCommandHandler(_salesNoteRepository, _contactRepository, _unitOfWork);
        var command = new UpdateSalesNoteCommand(note.Id, "Updated Title", "Need B", "Obj B", "Strat B", 85);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Need B", result.NeedAnalysis);
        Assert.Equal(85, result.Probability);

        _salesNoteRepository.Received(1).Update(note);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveSalesNoteCommandHandler_WithValidNote_ShouldSoftDeleteNote()
    {
        var note = SalesNote.Create(Guid.NewGuid(), "Title", "Need", "Obj", "Strat", 50, Guid.NewGuid());
        _salesNoteRepository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var handler = new ArchiveSalesNoteCommandHandler(_salesNoteRepository, _unitOfWork);
        var command = new ArchiveSalesNoteCommand(note.Id, Guid.NewGuid());

        await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(note.IsDeleted);
        _salesNoteRepository.Received(1).Update(note);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
