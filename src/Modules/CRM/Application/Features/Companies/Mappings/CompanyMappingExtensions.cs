using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Application.Features.Companies.DTOs;

public static class CompanyMappingExtensions
{
    public static CompanyDto ToDto(this Company company)
    {
        return new CompanyDto(
            company.Id,
            company.Name,
            company.Industry,
            company.Website,
            company.Phone,
            company.Email,
            new AddressDto(company.Address.Text),
            company.Type,
            company.CreatedAt,
            company.UpdatedAt,
            company.IsDeleted,
            company.DeletedAt
        );
    }

    public static CompanyListDto ToListDto(this Company company)
    {
        return new CompanyListDto(
            company.Id,
            company.Name,
            company.Industry,
            company.Website,
            company.Phone,
            company.Email,
            company.Type,
            company.CreatedAt
        );
    }
}
