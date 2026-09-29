using FluentValidation;
using Modules.CRM.Application.Features.SalesNotes.Commands;

namespace Modules.CRM.Application.Features.SalesNotes.Validators;

public class UpdateSalesNoteCommandValidator : AbstractValidator<UpdateSalesNoteCommand>
{
    public UpdateSalesNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("SalesNote Id is required.");

        RuleFor(x => x.Title)
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.NeedAnalysis)
            .MaximumLength(2000)
            .WithMessage("NeedAnalysis must not exceed 2000 characters.");

        RuleFor(x => x.Objections)
            .MaximumLength(2000)
            .WithMessage("Objections must not exceed 2000 characters.");

        RuleFor(x => x.Strategy)
            .MaximumLength(2000)
            .WithMessage("Strategy must not exceed 2000 characters.");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100)
            .WithMessage("Probability must be between 0 and 100.");

        RuleFor(x => x.CompetitorsMentioned)
            .MaximumLength(1000)
            .WithMessage("CompetitorsMentioned must not exceed 1000 characters.");

        RuleFor(x => x.BudgetInformation)
            .MaximumLength(1000)
            .WithMessage("BudgetInformation must not exceed 1000 characters.");

        RuleFor(x => x.DecisionMakerInfo)
            .MaximumLength(1000)
            .WithMessage("DecisionMakerInfo must not exceed 1000 characters.");
    }
}
