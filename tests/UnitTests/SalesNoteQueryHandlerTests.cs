using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.SalesNotes.Queries;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class SalesNoteQueryHandlerTests
{
    private readonly ISalesNoteRepository _salesNoteRepository = Substitute.For<ISalesNoteRepository>();
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();

    [Fact]
    public async Task GetSalesNoteByIdQueryHandler_WithExistingNote_ShouldReturnDto()
    {
        var note = SalesNote.Create(Guid.NewGuid(), "Discovery", "Needs React", "None", "Pitch architecture", 80, Guid.NewGuid());
        _salesNoteRepository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var handler = new GetSalesNoteByIdQueryHandler(_salesNoteRepository);
        var result = await handler.HandleAsync(new GetSalesNoteByIdQuery(note.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(note.Id, result.Id);
        Assert.Equal("Discovery", result.Title);
        Assert.Equal(80, result.Probability);
    }

    [Fact]
    public async Task GetLeadSalesNotesQueryHandler_WithValidLead_ShouldReturnLeadSalesContext()
    {
        var lead = Lead.Create(Guid.NewGuid(), "ERP Lead", "Referral");
        _leadRepository.GetByIdAsync(lead.Id, Arg.Any<CancellationToken>()).Returns(lead);

        var notes = new List<SalesNote>
        {
            SalesNote.Create(lead.Id, "Note 1", "Need 1", "Obj 1", "Strat 1", 60, Guid.NewGuid()),
            SalesNote.Create(lead.Id, "Note 2", "Need 2", "Obj 2", "Strat 2", 85, Guid.NewGuid())
        };

        _salesNoteRepository.CountAsync(lead.Id, null, null, null, null, Arg.Any<CancellationToken>()).Returns(2);
        _salesNoteRepository.GetListAsync(1, 20, lead.Id, null, null, null, null, Arg.Any<CancellationToken>()).Returns(notes);

        var handler = new GetLeadSalesNotesQueryHandler(_salesNoteRepository, _leadRepository);
        var result = await handler.HandleAsync(new GetLeadSalesNotesQuery(lead.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(lead.Id, result.LeadId);
        Assert.Equal(2, result.TotalSalesNotesCount);
        Assert.Equal(2, result.SalesNotes.Count);
    }
}
