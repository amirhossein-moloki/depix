using FluentValidation;
using Modules.CRM.Application.Features.Leads.Commands;

namespace Modules.CRM.Application.Features.Leads.Validators;

public class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Lead ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Lead title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Source)
            .NotEmpty().WithMessage("Lead source is required.")
            .MaximumLength(100).WithMessage("Source cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.EstimatedValue)
            .GreaterThanOrEqualTo(0).When(x => x.EstimatedValue.HasValue)
            .WithMessage("Estimated value cannot be negative.");
    }
}
