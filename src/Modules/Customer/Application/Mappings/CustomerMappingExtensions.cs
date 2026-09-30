using Modules.Customer.Application.DTOs;
using CustomerEntity = Modules.Customer.Domain.Entities.Customer;

namespace Modules.Customer.Application.Mappings;

public static class CustomerMappingExtensions
{
    public static CustomerDto ToDto(this CustomerEntity customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            CustomerNumber = customer.CustomerNumber,
            CustomerSince = customer.CustomerSince,
            Status = customer.Status,
            PrimaryContactId = customer.PrimaryContactId,
            AssignedTo = customer.AssignedTo,
            Notes = customer.Notes,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    public static CustomerListItemDto ToListItemDto(this CustomerEntity customer)
    {
        return new CustomerListItemDto
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            CustomerNumber = customer.CustomerNumber,
            CustomerSince = customer.CustomerSince,
            Status = customer.Status,
            PrimaryContactId = customer.PrimaryContactId,
            AssignedTo = customer.AssignedTo,
            CreatedAt = customer.CreatedAt
        };
    }

    public static CustomerDetailDto ToDetailDto(this CustomerEntity customer)
    {
        return new CustomerDetailDto
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            CustomerNumber = customer.CustomerNumber,
            CustomerSince = customer.CustomerSince,
            Status = customer.Status,
            PrimaryContactId = customer.PrimaryContactId,
            AssignedTo = customer.AssignedTo,
            Notes = customer.Notes,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
