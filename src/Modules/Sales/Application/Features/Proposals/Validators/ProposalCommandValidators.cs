using FluentValidation;
using Modules.Sales.Application.Features.Proposals.Commands;
using Modules.Sales.Domain.Constants;

namespace Modules.Sales.Application.Features.Proposals.Validators;

public class CreateProposalCommandValidator : AbstractValidator<CreateProposalCommand>
{
    public CreateProposalCommandValidator()
    {
        RuleFor(x => x.OpportunityId)
            .NotEmpty().WithMessage("Opportunity ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.ValidUntil)
            .NotEmpty().WithMessage("Valid until date is required.");

        RuleFor(x => x.Version)
            .MaximumLength(50).WithMessage("Version must not exceed 50 characters.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter code.");
    }
}

public class UpdateProposalCommandValidator : AbstractValidator<UpdateProposalCommand>
{
    public UpdateProposalCommandValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposal ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.ValidUntil)
            .NotEmpty().WithMessage("Valid until date is required.");
    }
}

public class AddProposalItemCommandValidator : AbstractValidator<AddProposalItemCommand>
{
    public AddProposalItemCommandValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposal ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(200).WithMessage("Item name must not exceed 200 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.");
    }
}

public class UpdateProposalItemCommandValidator : AbstractValidator<UpdateProposalItemCommand>
{
    public UpdateProposalItemCommandValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposal ID is required.");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("Item ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required.")
            .MaximumLength(200).WithMessage("Item name must not exceed 200 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.");
    }
}

public class ChangeProposalStatusCommandValidator : AbstractValidator<ChangeProposalStatusCommand>
{
    public ChangeProposalStatusCommandValidator()
    {
        RuleFor(x => x.ProposalId)
            .NotEmpty().WithMessage("Proposal ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(ProposalStatus.IsValid).WithMessage("Invalid proposal status.");
    }
}
