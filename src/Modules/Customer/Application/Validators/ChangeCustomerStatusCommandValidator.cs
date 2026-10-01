using FluentValidation;
using Modules.Customer.Application.Commands;

namespace Modules.Customer.Application.Validators;

public class ChangeCustomerStatusCommandValidator : AbstractValidator<ChangeCustomerStatusCommand>
{
    public ChangeCustomerStatusCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => s != null && (s.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase) ||
                                     s.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase) ||
                                     s.Equals("SUSPENDED", StringComparison.OrdinalIgnoreCase) ||
                                     s.Equals("ARCHIVED", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Status must be one of: ACTIVE, INACTIVE, SUSPENDED, ARCHIVED.");
    }
}
