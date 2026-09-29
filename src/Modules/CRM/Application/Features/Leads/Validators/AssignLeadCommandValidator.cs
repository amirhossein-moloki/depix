using FluentValidation;
using Modules.CRM.Application.Features.Leads.Commands;

namespace Modules.CRM.Application.Features.Leads.Validators;

public class AssignLeadCommandValidator : AbstractValidator<AssignLeadCommand>
{
    public AssignLeadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Lead ID is required.");
    }
}
