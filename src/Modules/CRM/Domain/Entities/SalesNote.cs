using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

public class SalesNote : Entity
{
    public Guid LeadId { get; private set; }
    public string NeedAnalysis { get; private set; } = string.Empty;
    public string Objections { get; private set; } = string.Empty;
    public string Strategy { get; private set; } = string.Empty;
    public int Probability { get; private set; }
    public Guid CreatedBy { get; private set; }

    private SalesNote() { }

    public SalesNote(Guid id, Guid leadId, string needAnalysis, string objections, string strategy, int probability, Guid createdBy) : base(id)
    {
        LeadId = leadId;
        NeedAnalysis = needAnalysis;
        Objections = objections;
        Strategy = strategy;
        Probability = probability;
        CreatedBy = createdBy;
    }

    public static SalesNote Create(Guid leadId, string needAnalysis, string objections, string strategy, int probability, Guid createdBy)
    {
        return new SalesNote(Guid.NewGuid(), leadId, needAnalysis, objections, strategy, probability, createdBy);
    }
}
