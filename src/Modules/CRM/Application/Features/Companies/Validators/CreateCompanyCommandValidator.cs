using FluentValidation;
using Modules.CRM.Application.Features.Companies.Commands;

namespace Modules.CRM.Application.Features.Companies.Validators;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200).WithMessage("Company name must not exceed 200 characters.");

        RuleFor(x => x.Industry)
            .MaximumLength(100).WithMessage("Industry must not exceed 100 characters.");

        RuleFor(x => x.Website)
            .MaximumLength(250).WithMessage("Website URL must not exceed 250 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Phone number must not exceed 50 characters.");

        RuleFor(x => x.Email)
            .MaximumLength(250).WithMessage("Email must not exceed 250 characters.")
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Type)
            .Must(type => string.IsNullOrWhiteSpace(type) || new[] { "LEAD", "CUSTOMER", "INACTIVE" }.Contains(type.ToUpperInvariant()))
            .WithMessage("Company type must be LEAD, CUSTOMER, or INACTIVE.");
    }
}
