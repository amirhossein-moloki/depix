using BuildingBlocks.Application.Contracts;
using FluentValidation;
using Modules.Customer.Application.Commands;

namespace Modules.Customer.Application.Validators;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    private readonly ICustomerContactService _customerContactService;

    public CreateCustomerCommandValidator(ICustomerContactService customerContactService)
    {
        _customerContactService = customerContactService;

        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required.");

        RuleFor(x => x.CustomerNumber)
            .MaximumLength(50).WithMessage("Customer number cannot exceed 50 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.");

        RuleFor(x => x.PrimaryContactId)
            .MustAsync(async (command, primaryContactId, cancellation) =>
            {
                if (!primaryContactId.HasValue) return true;
                var contact = await _customerContactService.GetContactByIdAsync(primaryContactId.Value, cancellation);
                if (contact == null) return false;
                return contact.CompanyId == command.CompanyId;
            })
            .WithMessage("Primary contact must exist and belong to the customer's company.");
    }
}
