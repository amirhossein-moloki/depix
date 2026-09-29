namespace Modules.CRM.Application.Features.Contacts.DTOs;

public record ContactDto(
    Guid Id,
    Guid CompanyId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Phone,
    string Position,
    string Description,
    bool IsDecisionMaker,
    string InfluenceLevel,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsDeleted,
    DateTime? DeletedAt
);

public record ContactListDto(
    Guid Id,
    Guid CompanyId,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string Phone,
    string Position,
    bool IsDecisionMaker,
    string InfluenceLevel,
    DateTime CreatedAt
);

public record CreateContactRequest(
    Guid CompanyId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Position,
    string Description,
    bool IsDecisionMaker = false,
    string InfluenceLevel = ""
);

public record UpdateContactRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Position,
    string Description,
    bool IsDecisionMaker = false,
    string InfluenceLevel = ""
);
