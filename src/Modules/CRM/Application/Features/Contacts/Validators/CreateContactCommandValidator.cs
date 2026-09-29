using FluentValidation;
using Modules.CRM.Application.Features.Contacts.Commands;

namespace Modules.CRM.Application.Features.Contacts.Validators;

public class CreateContactCommandValidator : AbstractValidator<CreateContactCommand>
{
    public CreateContactCommandValidator()
    {
        RuleFor(c => c.CompanyId)
            .NotEmpty()
            .WithMessage("Company ID is required.");

        RuleFor(c => c.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(c => c.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters.");

        RuleFor(c => c.Email)
            .MaximumLength(250)
            .WithMessage("Email must not exceed 250 characters.")
            .EmailAddress()
            .When(c => !string.IsNullOrWhiteSpace(c.Email))
            .WithMessage("Email format is invalid.");

        RuleFor(c => c.Phone)
            .MaximumLength(50)
            .WithMessage("Phone must not exceed 50 characters.");

        RuleFor(c => c.Position)
            .MaximumLength(100)
            .WithMessage("Position must not exceed 100 characters.");

        RuleFor(c => c.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters.");

        RuleFor(c => c.InfluenceLevel)
            .MaximumLength(50)
            .WithMessage("Influence level must not exceed 50 characters.");
    }
}
