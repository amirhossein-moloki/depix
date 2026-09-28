using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

public class LeadResearch : Entity
{
    public Guid LeadId { get; private set; }
    public string BusinessSummary { get; private set; } = string.Empty;
    public string PainPoints { get; private set; } = string.Empty;
    public string Opportunities { get; private set; } = string.Empty;
    public string Competitors { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;

    private LeadResearch() { }

    public LeadResearch(Guid id, Guid leadId, string businessSummary, string painPoints, string opportunities, string competitors, string notes) : base(id)
    {
        LeadId = leadId;
        BusinessSummary = businessSummary;
        PainPoints = painPoints;
        Opportunities = opportunities;
        Competitors = competitors;
        Notes = notes;
    }

    public static LeadResearch Create(Guid leadId, string businessSummary, string painPoints, string opportunities, string competitors, string notes)
    {
        return new LeadResearch(Guid.NewGuid(), leadId, businessSummary, painPoints, opportunities, competitors, notes);
    }
}
