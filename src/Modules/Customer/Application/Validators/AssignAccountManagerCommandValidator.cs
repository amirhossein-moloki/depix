using FluentValidation;
using Modules.Customer.Application.Commands;

namespace Modules.Customer.Application.Validators;

public class AssignAccountManagerCommandValidator : AbstractValidator<AssignAccountManagerCommand>
{
    public AssignAccountManagerCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
    }
}
