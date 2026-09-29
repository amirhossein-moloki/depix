using Modules.CRM.Application.Features.SalesNotes.DTOs;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Application.Features.SalesNotes.Mappings;

public static class SalesNoteMappingExtensions
{
    public static SalesNoteDto ToDto(this SalesNote salesNote)
    {
        return new SalesNoteDto(
            salesNote.Id,
            salesNote.LeadId,
            salesNote.Title,
            salesNote.NeedAnalysis,
            salesNote.Objections,
            salesNote.Strategy,
            salesNote.Probability,
            salesNote.CompanyId,
            salesNote.ContactId,
            salesNote.CompetitorsMentioned,
            salesNote.BudgetInformation,
            salesNote.DecisionMakerInfo,
            salesNote.CreatedAt,
            salesNote.CreatedBy,
            salesNote.UpdatedAt,
            salesNote.UpdatedBy,
            salesNote.IsDeleted
        );
    }

    public static SalesNoteListItemDto ToListItemDto(this SalesNote salesNote)
    {
        return new SalesNoteListItemDto(
            salesNote.Id,
            salesNote.LeadId,
            salesNote.Title,
            salesNote.Probability,
            salesNote.CreatedAt,
            salesNote.CreatedBy
        );
    }
}
