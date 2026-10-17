using FluentValidation;
using Modules.CRM.Application.Features.SalesVisits.Commands;

namespace Modules.CRM.Application.Features.SalesVisits.Validators;

public class CreateSalesVisitCommandValidator : AbstractValidator<CreateSalesVisitCommand>
{
    public CreateSalesVisitCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Company ID is required.");

        RuleFor(x => x.SalespersonId)
            .NotEmpty().WithMessage("Salesperson ID is required.");

        RuleFor(x => x.ReportText)
            .NotEmpty().WithMessage("Visit report text is required.")
            .MaximumLength(2000).WithMessage("Report text must not exceed 2000 characters.");

        RuleFor(x => x.Result)
            .NotEmpty().WithMessage("Visit result is required.")
            .MaximumLength(200).WithMessage("Result must not exceed 200 characters.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90.0, 90.0).When(x => x.Latitude.HasValue)
            .WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180.0, 180.0).When(x => x.Longitude.HasValue)
            .WithMessage("Longitude must be between -180 and 180 degrees.");

        RuleFor(x => x)
            .Must(x => x.Latitude.HasValue == x.Longitude.HasValue)
            .WithMessage("Both latitude and longitude must be provided together, or both must be null.");
    }
}
