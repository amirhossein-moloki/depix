using FluentValidation;
using Modules.CRM.Application.Features.Leads.Commands;

namespace Modules.CRM.Application.Features.Leads.Validators;

public class SubmitInstagramLeadCommandValidator : AbstractValidator<SubmitInstagramLeadCommand>
{
    public SubmitInstagramLeadCommandValidator()
    {
        RuleFor(x => x.BrandName)
            .NotEmpty().WithMessage("Brand name is required.")
            .MaximumLength(150).WithMessage("Brand name cannot exceed 150 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(50).WithMessage("Phone number cannot exceed 50 characters.");

        RuleFor(x => x.InstagramId)
            .NotEmpty().WithMessage("Instagram ID is required.")
            .MaximumLength(100).WithMessage("Instagram ID cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");
    }
}
