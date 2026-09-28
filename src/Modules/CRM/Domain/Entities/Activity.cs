using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

public class Activity : AuditableEntity
{
    public Guid LeadId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Type { get; private set; } = string.Empty; // CALL, EMAIL, MEETING, MESSAGE
    public string Description { get; private set; } = string.Empty;
    public string Result { get; private set; } = string.Empty;
    public int QualityScore { get; private set; }

    private Activity() { }

    public Activity(Guid id, Guid leadId, Guid? userId, string type, string description, string result, int qualityScore) : base(id)
    {
        LeadId = leadId;
        UserId = userId;
        Type = type;
        Description = description;
        Result = result;
        QualityScore = qualityScore;
    }

    public static Activity Create(Guid leadId, Guid? userId, string type, string description, string result, int qualityScore)
    {
        return new Activity(Guid.NewGuid(), leadId, userId, type, description, result, qualityScore);
    }
}
