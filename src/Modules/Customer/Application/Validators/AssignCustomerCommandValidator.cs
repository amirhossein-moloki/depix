using FluentValidation;
using Modules.Customer.Application.Commands;

namespace Modules.Customer.Application.Validators;

public class AssignCustomerCommandValidator : AbstractValidator<AssignCustomerCommand>
{
    public AssignCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
