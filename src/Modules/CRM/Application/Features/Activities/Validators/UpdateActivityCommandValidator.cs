using FluentValidation;
using Modules.CRM.Application.Features.Activities.Commands;

namespace Modules.CRM.Application.Features.Activities.Validators;

public class UpdateActivityCommandValidator : AbstractValidator<UpdateActivityCommand>
{
    public UpdateActivityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Activity Id is required.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Activity type is required.")
            .MaximumLength(50)
            .WithMessage("Activity type must not exceed 50 characters.");

        RuleFor(x => x.Subject)
            .MaximumLength(200)
            .WithMessage("Subject must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Result)
            .MaximumLength(500)
            .WithMessage("Result must not exceed 500 characters.");

        RuleFor(x => x.QualityScore)
            .InclusiveBetween(0, 100)
            .WithMessage("QualityScore must be between 0 and 100.");

        RuleFor(x => x.FollowUpNotes)
            .MaximumLength(1000)
            .WithMessage("FollowUpNotes must not exceed 1000 characters.");
    }
}
