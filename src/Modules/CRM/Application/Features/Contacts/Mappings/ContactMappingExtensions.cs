using Modules.CRM.Application.Features.Contacts.DTOs;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Application.Features.Contacts.Mappings;

public static class ContactMappingExtensions
{
    public static ContactDto ToDto(this Contact contact)
    {
        var fullName = string.IsNullOrWhiteSpace($"{contact.FirstName} {contact.LastName}".Trim())
            ? (contact.Name ?? string.Empty)
            : $"{contact.FirstName} {contact.LastName}".Trim();

        return new ContactDto(
            contact.Id,
            contact.CompanyId,
            contact.FirstName,
            contact.LastName,
            fullName,
            contact.Email,
            contact.Phone,
            contact.Position,
            contact.Description,
            contact.IsDecisionMaker,
            contact.InfluenceLevel,
            contact.CreatedAt,
            contact.UpdatedAt,
            contact.IsDeleted,
            contact.DeletedAt
        );
    }

    public static ContactListDto ToListDto(this Contact contact)
    {
        var fullName = string.IsNullOrWhiteSpace($"{contact.FirstName} {contact.LastName}".Trim())
            ? (contact.Name ?? string.Empty)
            : $"{contact.FirstName} {contact.LastName}".Trim();

        return new ContactListDto(
            contact.Id,
            contact.CompanyId,
            contact.FirstName,
            contact.LastName,
            fullName,
            contact.Email,
            contact.Phone,
            contact.Position,
            contact.IsDecisionMaker,
            contact.InfluenceLevel,
            contact.CreatedAt
        );
    }
}
