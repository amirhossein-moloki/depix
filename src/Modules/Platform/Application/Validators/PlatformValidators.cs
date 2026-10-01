using FluentValidation;
using Modules.Platform.Application.DTOs;

namespace Modules.Platform.Application.Validators;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("EntityType is required.")
            .MaximumLength(100).WithMessage("EntityType cannot exceed 100 characters.");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("EntityId is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("FileName is required.")
            .MaximumLength(250).WithMessage("FileName cannot exceed 250 characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("ContentType is required.");

        RuleFor(x => x.Size)
            .GreaterThan(0).WithMessage("File size must be greater than 0.")
            .LessThanOrEqualTo(100 * 1024 * 1024).WithMessage("File size cannot exceed 100 MB.");
    }
}

public class CreateSettingCommandValidator : AbstractValidator<CreateSettingCommand>
{
    public CreateSettingCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting Key is required.")
            .MaximumLength(100).WithMessage("Setting Key cannot exceed 100 characters.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Setting Value is required.");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters.");
    }
}

public class UpdateSettingCommandValidator : AbstractValidator<UpdateSettingCommand>
{
    public UpdateSettingCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting Key is required.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Setting Value is required.");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters.");
    }
}

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .MaximumLength(50).WithMessage("Priority cannot exceed 50 characters.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .MaximumLength(50).WithMessage("Status cannot exceed 50 characters.");
    }
}

public class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");
    }
}
