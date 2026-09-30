using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.Customer.Application.DTOs;
using Modules.Customer.Application.Mappings;
using Modules.Customer.Domain.Repositories;
using CustomerEntity = Modules.Customer.Domain.Entities.Customer;

namespace Modules.Customer.Application.Commands;

public record CreateCustomerCommand(
    Guid CompanyId,
    string? CustomerNumber,
    DateOnly? CustomerSince,
    Guid? PrimaryContactId,
    Guid? AssignedTo,
    string? Notes) : ICommand<CustomerDto>;

public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        if (await _customerRepository.ExistsForCompanyAsync(command.CompanyId, cancellationToken))
        {
            throw new BusinessRuleException("A customer record already exists for this company.");
        }

        string customerNumber;
        if (!string.IsNullOrWhiteSpace(command.CustomerNumber))
        {
            customerNumber = command.CustomerNumber.Trim();
            var existingByNumber = await _customerRepository.GetByCustomerNumberAsync(customerNumber, cancellationToken);
            if (existingByNumber != null)
            {
                throw new BusinessRuleException($"Customer number '{customerNumber}' is already in use.");
            }
        }
        else
        {
            var year = DateTime.UtcNow.Year;
            var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
            customerNumber = $"CUST-{year}-{randomPart}";
        }

        var customer = CustomerEntity.Create(
            command.CompanyId,
            customerNumber,
            command.CustomerSince,
            command.PrimaryContactId,
            command.AssignedTo,
            command.Notes);

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return customer.ToDto();
    }
}
