using BuildingBlocks.Application.Contracts;
using FluentValidation;
using Modules.Customer.Application.Commands;
using Modules.Customer.Domain.Repositories;

namespace Modules.Customer.Application.Validators;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerContactService _customerContactService;

    public UpdateCustomerCommandValidator(
        ICustomerRepository customerRepository,
        ICustomerContactService customerContactService)
    {
        _customerRepository = customerRepository;
        _customerContactService = customerContactService;

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) ||
                            status.Trim().Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) ||
                            status.Trim().Equals("INACTIVE", StringComparison.OrdinalIgnoreCase) ||
                            status.Trim().Equals("ARCHIVED", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Status must be ACTIVE, INACTIVE, or ARCHIVED.");

        RuleFor(x => x.PrimaryContactId)
            .MustAsync(async (command, primaryContactId, cancellation) =>
            {
                if (!primaryContactId.HasValue) return true;
                var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellation);
                if (customer == null) return false;

                var contact = await _customerContactService.GetContactByIdAsync(primaryContactId.Value, cancellation);
                if (contact == null) return false;

                return contact.CompanyId == customer.CompanyId;
            })
            .WithMessage("Primary contact must exist and belong to the customer's company.");
    }
}
