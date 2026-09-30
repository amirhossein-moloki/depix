using FluentValidation;
using Modules.Sales.Application.Features.Opportunities.Commands;
using Modules.Sales.Domain.Constants;

namespace Modules.Sales.Application.Features.Opportunities.Validators;

public class CreateOpportunityCommandValidator : AbstractValidator<CreateOpportunityCommand>
{
    public CreateOpportunityCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.ValueAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Value amount cannot be negative.");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).When(x => x.Probability.HasValue)
            .WithMessage("Probability must be between 0 and 100.");

        RuleFor(x => x.Stage)
            .Must(stage => string.IsNullOrWhiteSpace(stage) || OpportunityStage.IsValid(stage))
            .WithMessage("Invalid stage provided.");
    }
}
