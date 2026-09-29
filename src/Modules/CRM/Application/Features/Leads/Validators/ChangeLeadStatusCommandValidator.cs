using FluentValidation;
using Modules.CRM.Application.Features.Leads.Commands;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Application.Features.Leads.Validators;

public class ChangeLeadStatusCommandValidator : AbstractValidator<ChangeLeadStatusCommand>
{
    public ChangeLeadStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Lead ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => Lead.ValidStatuses.Contains(status.Trim()))
            .WithMessage("Invalid status value.");

        RuleFor(x => x.Reason)
            .NotEmpty().When(x => x.Status != null && x.Status.Trim().Equals("DISQUALIFIED", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Disqualification reason is required when marking lead as DISQUALIFIED.");
    }
}
