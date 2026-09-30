using FluentValidation;
using Modules.Sales.Application.Features.Opportunities.Commands;

namespace Modules.Sales.Application.Features.Opportunities.Validators;

public class UpdateOpportunityCommandValidator : AbstractValidator<UpdateOpportunityCommand>
{
    public UpdateOpportunityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Opportunity ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.ValueAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Value amount cannot be negative.");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100.");
    }
}
