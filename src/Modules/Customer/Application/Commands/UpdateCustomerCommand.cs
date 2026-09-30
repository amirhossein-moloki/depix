using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Customer.Application.DTOs;
using Modules.Customer.Application.Mappings;
using Modules.Customer.Domain.Repositories;

namespace Modules.Customer.Application.Commands;

public record UpdateCustomerCommand(
    Guid CustomerId,
    Guid? PrimaryContactId,
    string? Notes,
    string? Status) : ICommand<CustomerDto>;

public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> HandleAsync(UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
        if (customer == null || customer.IsDeleted)
        {
            throw new EntityNotFoundException("Customer", command.CustomerId);
        }

        customer.UpdateDetails(command.PrimaryContactId, command.Notes);

        if (!string.IsNullOrWhiteSpace(command.Status))
        {
            customer.UpdateStatus(command.Status);
        }

        _customerRepository.Update(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToDto();
    }
}
