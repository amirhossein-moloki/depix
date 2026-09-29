using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

/// <summary>
/// Represents internal CRM pre-sales knowledge, requirements analysis, objections, and strategies for a lead.
/// </summary>
public class SalesNote : AuditableEntity, ISoftDelete
{
    public Guid LeadId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string NeedAnalysis { get; private set; } = string.Empty;
    public string Objections { get; private set; } = string.Empty;
    public string Strategy { get; private set; } = string.Empty;
    public int Probability { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string? CompetitorsMentioned { get; private set; }
    public string? BudgetInformation { get; private set; }
    public string? DecisionMakerInfo { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private SalesNote() { }

    public SalesNote(
        Guid id,
        Guid leadId,
        string title,
        string needAnalysis,
        string objections,
        string strategy,
        int probability,
        Guid createdBy,
        Guid? companyId = null,
        Guid? contactId = null,
        string? competitorsMentioned = null,
        string? budgetInformation = null,
        string? decisionMakerInfo = null) : base(id)
    {
        LeadId = leadId;
        Title = string.IsNullOrWhiteSpace(title) ? "Sales Note" : title.Trim();
        NeedAnalysis = needAnalysis ?? string.Empty;
        Objections = objections ?? string.Empty;
        Strategy = strategy ?? string.Empty;
        Probability = Math.Clamp(probability, 0, 100);
        CompanyId = companyId;
        ContactId = contactId;
        CompetitorsMentioned = competitorsMentioned;
        BudgetInformation = budgetInformation;
        DecisionMakerInfo = decisionMakerInfo;
        SetCreator(createdBy);
    }

    public static SalesNote Create(
        Guid leadId,
        string needAnalysis,
        string objections,
        string strategy,
        int probability,
        Guid createdBy)
    {
        return Create(leadId, "Sales Note", needAnalysis, objections, strategy, probability, createdBy);
    }

    public static SalesNote Create(
        Guid leadId,
        string title,
        string needAnalysis,
        string objections,
        string strategy,
        int probability,
        Guid createdBy,
        Guid? companyId = null,
        Guid? contactId = null,
        string? competitorsMentioned = null,
        string? budgetInformation = null,
        string? decisionMakerInfo = null)
    {
        if (leadId == Guid.Empty)
        {
            throw new ArgumentException("Lead ID is required.", nameof(leadId));
        }

        return new SalesNote(
            Guid.NewGuid(),
            leadId,
            title,
            needAnalysis,
            objections,
            strategy,
            probability,
            createdBy,
            companyId,
            contactId,
            competitorsMentioned,
            budgetInformation,
            decisionMakerInfo);
    }

    public void UpdateInformation(
        string title,
        string needAnalysis,
        string objections,
        string strategy,
        int probability,
        Guid? companyId = null,
        Guid? contactId = null,
        string? competitorsMentioned = null,
        string? budgetInformation = null,
        string? decisionMakerInfo = null)
    {
        Title = string.IsNullOrWhiteSpace(title) ? Title : title.Trim();
        NeedAnalysis = needAnalysis ?? NeedAnalysis;
        Objections = objections ?? Objections;
        Strategy = strategy ?? Strategy;
        Probability = Math.Clamp(probability, 0, 100);
        CompanyId = companyId ?? CompanyId;
        ContactId = contactId ?? ContactId;
        CompetitorsMentioned = competitorsMentioned ?? CompetitorsMentioned;
        BudgetInformation = budgetInformation ?? BudgetInformation;
        DecisionMakerInfo = decisionMakerInfo ?? DecisionMakerInfo;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
