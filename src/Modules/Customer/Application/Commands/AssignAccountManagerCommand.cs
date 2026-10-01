using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Customer.Application.DTOs;
using Modules.Customer.Application.Mappings;
using Modules.Customer.Domain.Repositories;

namespace Modules.Customer.Application.Commands;

public record AssignAccountManagerCommand(
    Guid CustomerId,
    Guid? AccountManagerId) : ICommand<CustomerDto>;

public class AssignAccountManagerCommandHandler : ICommandHandler<AssignAccountManagerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignAccountManagerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> HandleAsync(AssignAccountManagerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
        if (customer == null || customer.IsDeleted)
        {
            throw new EntityNotFoundException("Customer", command.CustomerId);
        }

        customer.Assign(command.AccountManagerId);

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToDto();
    }
}
