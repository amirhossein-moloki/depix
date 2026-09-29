using BuildingBlocks.Application.CQRS;

namespace Modules.CRM.Application.Features.SalesNotes.DTOs;

public record SalesNoteDto(
    Guid Id,
    Guid LeadId,
    string Title,
    string NeedAnalysis,
    string Objections,
    string Strategy,
    int Probability,
    Guid? CompanyId,
    Guid? ContactId,
    string? CompetitorsMentioned,
    string? BudgetInformation,
    string? DecisionMakerInfo,
    DateTime CreatedAt,
    Guid? CreatedBy,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    bool IsDeleted
);

public record SalesNoteListItemDto(
    Guid Id,
    Guid LeadId,
    string Title,
    int Probability,
    DateTime CreatedAt,
    Guid? CreatedBy
);

public record LeadSalesContextDto(
    Guid LeadId,
    int TotalSalesNotesCount,
    int LatestProbability,
    IReadOnlyList<SalesNoteDto> SalesNotes
);

public record CreateSalesNoteRequest(
    Guid LeadId,
    string Title,
    string NeedAnalysis,
    string Objections,
    string Strategy,
    int Probability,
    Guid? CompanyId = null,
    Guid? ContactId = null,
    string? CompetitorsMentioned = null,
    string? BudgetInformation = null,
    string? DecisionMakerInfo = null
);

public record UpdateSalesNoteRequest(
    string Title,
    string NeedAnalysis,
    string Objections,
    string Strategy,
    int Probability,
    Guid? CompanyId = null,
    Guid? ContactId = null,
    string? CompetitorsMentioned = null,
    string? BudgetInformation = null,
    string? DecisionMakerInfo = null
);
