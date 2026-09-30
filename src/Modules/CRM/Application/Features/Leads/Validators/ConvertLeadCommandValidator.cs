using FluentValidation;
using Modules.CRM.Application.Features.Leads.Commands;

namespace Modules.CRM.Application.Features.Leads.Validators;

public class ConvertLeadCommandValidator : AbstractValidator<ConvertLeadCommand>
{
    public ConvertLeadCommandValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty()
            .WithMessage("Lead ID is required.");
    }
}
