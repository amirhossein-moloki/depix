using FluentValidation;
using Modules.Sales.Application.Features.Opportunities.Commands;

namespace Modules.Sales.Application.Features.Opportunities.Validators;

public class MarkOpportunityLostCommandValidator : AbstractValidator<MarkOpportunityLostCommand>
{
    public MarkOpportunityLostCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Opportunity ID is required.");

        RuleFor(x => x.LossReason)
            .NotEmpty().WithMessage("Loss reason is required.")
            .MaximumLength(500).WithMessage("Loss reason cannot exceed 500 characters.");
    }
}
