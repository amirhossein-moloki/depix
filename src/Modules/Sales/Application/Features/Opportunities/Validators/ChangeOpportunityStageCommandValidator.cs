using FluentValidation;
using Modules.Sales.Application.Features.Opportunities.Commands;
using Modules.Sales.Domain.Constants;

namespace Modules.Sales.Application.Features.Opportunities.Validators;

public class ChangeOpportunityStageCommandValidator : AbstractValidator<ChangeOpportunityStageCommand>
{
    public ChangeOpportunityStageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Opportunity ID is required.");

        RuleFor(x => x.Stage)
            .NotEmpty().WithMessage("Stage is required.")
            .Must(OpportunityStage.IsValid).WithMessage("Invalid stage provided.");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).When(x => x.Probability.HasValue)
            .WithMessage("Probability must be between 0 and 100.");
    }
}
