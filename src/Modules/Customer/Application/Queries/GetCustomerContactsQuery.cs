using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Customer.Application.DTOs;
using Modules.Customer.Application.Mappings;
using Modules.Customer.Domain.Repositories;
using BuildingBlocks.Application.Contracts;

namespace Modules.Customer.Application.Queries;

public record GetCustomerContactsQuery(Guid CustomerId) : IQuery<List<CustomerContactContractDto>>;

public class GetCustomerContactsQueryHandler : IQueryHandler<GetCustomerContactsQuery, List<CustomerContactContractDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerContactService _customerContactService;

    public GetCustomerContactsQueryHandler(
        ICustomerRepository customerRepository,
        ICustomerContactService customerContactService)
    {
        _customerRepository = customerRepository;
        _customerContactService = customerContactService;
    }

    public async Task<List<CustomerContactContractDto>> HandleAsync(GetCustomerContactsQuery query, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(query.CustomerId, cancellationToken);
        if (customer == null || customer.IsDeleted)
        {
            throw new EntityNotFoundException("Customer", query.CustomerId);
        }

        return await _customerContactService.GetContactsByCompanyIdAsync(customer.CompanyId, cancellationToken);
    }
}
