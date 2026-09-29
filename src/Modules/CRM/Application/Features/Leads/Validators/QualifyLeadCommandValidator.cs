using FluentValidation;
using Modules.CRM.Application.Features.Leads.Commands;

namespace Modules.CRM.Application.Features.Leads.Validators;

public class QualifyLeadCommandValidator : AbstractValidator<QualifyLeadCommand>
{
    public QualifyLeadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Lead ID is required.");
    }
}
