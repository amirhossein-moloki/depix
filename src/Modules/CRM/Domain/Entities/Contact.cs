using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

public class Contact : AuditableEntity
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Position { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsDecisionMaker { get; private set; }
    public string InfluenceLevel { get; private set; } = string.Empty;

    private Contact() { }

    public Contact(Guid id, Guid companyId, string name, string position, string phone, string email, bool isDecisionMaker, string influenceLevel) : base(id)
    {
        CompanyId = companyId;
        Name = name;
        Position = position;
        Phone = phone;
        Email = email;
        IsDecisionMaker = isDecisionMaker;
        InfluenceLevel = influenceLevel;
    }

    public static Contact Create(Guid companyId, string name, string position, string phone, string email, bool isDecisionMaker, string influenceLevel)
    {
        return new Contact(Guid.NewGuid(), companyId, name, position, phone, email, isDecisionMaker, influenceLevel);
    }
}
