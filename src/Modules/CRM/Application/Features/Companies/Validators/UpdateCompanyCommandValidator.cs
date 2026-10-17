using FluentValidation;
using Modules.CRM.Application.Features.Companies.Commands;

namespace Modules.CRM.Application.Features.Companies.Validators;

public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Latitude.HasValue == x.Longitude.HasValue)
            .WithMessage("Latitude and longitude must be supplied together.");
        RuleFor(x => x.Latitude)
            .Must(value => !value.HasValue || (value.Value >= -90 && value.Value <= 90))
            .WithMessage("Latitude must be between -90 and 90.");
        RuleFor(x => x.Longitude)
            .Must(value => !value.HasValue || (value.Value >= -180 && value.Value <= 180))
            .WithMessage("Longitude must be between -180 and 180.");
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Company ID is required.");

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
            .NotEmpty().WithMessage("Company type is required.")
            .Must(type => new[] { "LEAD", "CUSTOMER", "INACTIVE" }.Contains(type.ToUpperInvariant()))
            .WithMessage("Company type must be LEAD, CUSTOMER, or INACTIVE.");
    }
}
