using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.Entities;

public class Contact : AuditableEntity, ISoftDelete
{
    public Guid CompanyId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Position { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsDecisionMaker { get; private set; }
    public string InfluenceLevel { get; private set; } = string.Empty;

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private Contact() { }

    public Contact(
        Guid id,
        Guid companyId,
        string firstName,
        string lastName,
        string email,
        string phone,
        string position,
        string description,
        bool isDecisionMaker = false,
        string influenceLevel = "") : base(id)
    {
        CompanyId = companyId;
        FirstName = firstName;
        LastName = lastName;
        Name = string.IsNullOrWhiteSpace($"{firstName} {lastName}".Trim()) ? (firstName ?? string.Empty) : $"{firstName} {lastName}".Trim();
        Email = email;
        Phone = phone;
        Position = position;
        Description = description;
        IsDecisionMaker = isDecisionMaker;
        InfluenceLevel = influenceLevel;
    }

    public static Contact Create(
        Guid companyId,
        string firstName,
        string lastName,
        string email,
        string phone,
        string position,
        string description,
        bool isDecisionMaker = false,
        string influenceLevel = "")
    {
        return new Contact(
            Guid.NewGuid(),
            companyId,
            firstName,
            lastName,
            email,
            phone,
            position,
            description,
            isDecisionMaker,
            influenceLevel);
    }

    public void UpdateInformation(
        string firstName,
        string lastName,
        string email,
        string phone,
        string position,
        string description,
        bool isDecisionMaker,
        string influenceLevel)
    {
        FirstName = firstName;
        LastName = lastName;
        Name = string.IsNullOrWhiteSpace($"{firstName} {lastName}".Trim()) ? (firstName ?? string.Empty) : $"{firstName} {lastName}".Trim();
        Email = email;
        Phone = phone;
        Position = position;
        Description = description;
        IsDecisionMaker = isDecisionMaker;
        InfluenceLevel = influenceLevel;
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
