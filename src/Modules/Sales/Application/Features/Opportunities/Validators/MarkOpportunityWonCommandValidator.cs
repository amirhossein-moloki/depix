using FluentValidation;
using Modules.Sales.Application.Features.Opportunities.Commands;

namespace Modules.Sales.Application.Features.Opportunities.Validators;

public class MarkOpportunityWonCommandValidator : AbstractValidator<MarkOpportunityWonCommand>
{
    public MarkOpportunityWonCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Opportunity ID is required.");
    }
}
