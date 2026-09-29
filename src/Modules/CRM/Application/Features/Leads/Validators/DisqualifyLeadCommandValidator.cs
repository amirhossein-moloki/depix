using FluentValidation;
using Modules.CRM.Application.Features.Leads.Commands;

namespace Modules.CRM.Application.Features.Leads.Validators;

public class DisqualifyLeadCommandValidator : AbstractValidator<DisqualifyLeadCommand>
{
    public DisqualifyLeadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Lead ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Disqualification reason is required.")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
