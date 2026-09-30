using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Customer.Application.DTOs;
using Modules.Customer.Application.Mappings;
using Modules.Customer.Domain.Repositories;

namespace Modules.Customer.Application.Queries;

public record GetCustomerByCompanyQuery(Guid CompanyId) : IQuery<CustomerDetailDto>;

public class GetCustomerByCompanyQueryHandler : IQueryHandler<GetCustomerByCompanyQuery, CustomerDetailDto>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByCompanyQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDetailDto> HandleAsync(GetCustomerByCompanyQuery query, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByCompanyIdAsync(query.CompanyId, cancellationToken);
        if (customer == null || customer.IsDeleted)
        {
            throw new EntityNotFoundException("Customer for Company", query.CompanyId);
        }

        return customer.ToDetailDto();
    }
}
