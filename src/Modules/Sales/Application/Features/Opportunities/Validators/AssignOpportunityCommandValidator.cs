using FluentValidation;
using Modules.Sales.Application.Features.Opportunities.Commands;

namespace Modules.Sales.Application.Features.Opportunities.Validators;

public class AssignOpportunityCommandValidator : AbstractValidator<AssignOpportunityCommand>
{
    public AssignOpportunityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Opportunity ID is required.");
    }
}
