using FluentValidation;
using Modules.Support.Application.Commands;
using Modules.Support.Domain.Constants;

namespace Modules.Support.Application.Validators;

public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.Priority)
            .Must(TicketPriority.IsValid)
            .WithMessage("Priority is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Priority));

        RuleFor(x => x.Category)
            .Must(TicketCategory.IsValid)
            .WithMessage("Category is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Category));
    }
}

public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
{
    public UpdateTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(200).WithMessage("Subject cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.Category)
            .Must(TicketCategory.IsValid)
            .WithMessage("Category is invalid.")
            .When(x => !string.IsNullOrWhiteSpace(x.Category));
    }
}

public class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");
    }
}

public class ChangeTicketPriorityCommandValidator : AbstractValidator<ChangeTicketPriorityCommand>
{
    public ChangeTicketPriorityCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(TicketPriority.IsValid).WithMessage("Priority is invalid.");
    }
}

public class ChangeTicketStatusCommandValidator : AbstractValidator<ChangeTicketStatusCommand>
{
    public ChangeTicketStatusCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(TicketStatus.IsValid).WithMessage("Status is invalid.");
    }
}

public class ResolveTicketCommandValidator : AbstractValidator<ResolveTicketCommand>
{
    public ResolveTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");

        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("Resolution is required.");
    }
}

public class AddTicketCommentCommandValidator : AbstractValidator<AddTicketCommentCommand>
{
    public AddTicketCommentCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("TicketId is required.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Comment message is required.")
            .MaximumLength(4000).WithMessage("Comment message cannot exceed 4000 characters.");
    }
}
