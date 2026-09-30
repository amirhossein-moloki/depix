using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Customer.Application.DTOs;
using Modules.Customer.Application.Mappings;
using Modules.Customer.Domain.Repositories;

namespace Modules.Customer.Application.Queries;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDetailDto>;

public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerDetailDto>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDetailDto> HandleAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(query.Id, cancellationToken);
        if (customer == null || customer.IsDeleted)
        {
            throw new EntityNotFoundException("Customer", query.Id);
        }

        return customer.ToDetailDto();
    }
}
