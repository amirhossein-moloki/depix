using FluentValidation;
using Modules.Finance.Application.Features.Invoices.Commands;
using Modules.Finance.Domain.Constants;

namespace Modules.Finance.Application.Validators;

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-character ISO code.");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .When(x => x.IssueDate.HasValue && x.DueDate.HasValue)
            .WithMessage("DueDate cannot be earlier than IssueDate.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description)
                .NotEmpty().WithMessage("Item description is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThanOrEqualTo(0m).WithMessage("UnitPrice cannot be negative.");

            item.RuleFor(i => i.Discount)
                .GreaterThanOrEqualTo(0m).WithMessage("Discount cannot be negative.");

            item.RuleFor(i => i.TaxRatePercentage)
                .GreaterThanOrEqualTo(0m).WithMessage("TaxRatePercentage cannot be negative.");
        });
    }
}

public class UpdateInvoiceCommandValidator : AbstractValidator<UpdateInvoiceCommand>
{
    public UpdateInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("InvoiceId is required.");

        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("DueDate cannot be earlier than IssueDate.");
    }
}

public class AddInvoiceItemCommandValidator : AbstractValidator<AddInvoiceItemCommand>
{
    public AddInvoiceItemCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("InvoiceId is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Item description is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0m).WithMessage("UnitPrice cannot be negative.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0m).WithMessage("Discount cannot be negative.");

        RuleFor(x => x.TaxRatePercentage)
            .GreaterThanOrEqualTo(0m).WithMessage("TaxRatePercentage cannot be negative.");
    }
}

public class UpdateInvoiceItemCommandValidator : AbstractValidator<UpdateInvoiceItemCommand>
{
    public UpdateInvoiceItemCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("InvoiceId is required.");

        RuleFor(x => x.ItemId)
            .NotEmpty().WithMessage("ItemId is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Item description is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0m).WithMessage("UnitPrice cannot be negative.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0m).WithMessage("Discount cannot be negative.");

        RuleFor(x => x.TaxRatePercentage)
            .GreaterThanOrEqualTo(0m).WithMessage("TaxRatePercentage cannot be negative.");
    }
}

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId)
            .NotEmpty().WithMessage("InvoiceId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0m).WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-character ISO code.");

        RuleFor(x => x.Method)
            .NotEmpty().WithMessage("Payment method is required.")
            .Must(m => PaymentMethod.All.Contains(m.ToUpperInvariant()))
            .WithMessage($"Invalid payment method. Allowed values: {string.Join(", ", PaymentMethod.All)}");
    }
}
