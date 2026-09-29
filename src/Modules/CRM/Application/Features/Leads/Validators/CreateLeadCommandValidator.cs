using FluentValidation;
using Modules.CRM.Application.Features.Leads.Commands;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Application.Features.Leads.Validators;

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required.");

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

        RuleFor(x => x.Score)
            .GreaterThanOrEqualTo(0).WithMessage("Score cannot be negative.");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || Lead.ValidStatuses.Contains(status.Trim()))
            .WithMessage("Invalid status value.");
    }
}
