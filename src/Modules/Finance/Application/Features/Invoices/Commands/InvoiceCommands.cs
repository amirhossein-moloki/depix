using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Finance.Application.DTOs;
using Modules.Finance.Application.Mappings;
using Modules.Finance.Application.Services;
using Modules.Finance.Domain.Entities;
using Modules.Finance.Domain.Repositories;

namespace Modules.Finance.Application.Features.Invoices.Commands;

public record CreateInvoiceCommand(
    Guid CustomerId,
    Guid? ProjectId,
    Guid? OpportunityId,
    Guid? ProposalId,
    DateOnly? IssueDate,
    DateOnly? DueDate,
    string Currency,
    string Notes,
    List<CreateInvoiceItemRequest>? Items) : ICommand<InvoiceDto>;

public record UpdateInvoiceCommand(
    Guid InvoiceId,
    Guid? ProjectId,
    Guid? OpportunityId,
    Guid? ProposalId,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Notes) : ICommand<InvoiceDto>;

public record AddInvoiceItemCommand(
    Guid InvoiceId,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal TaxRatePercentage) : ICommand<InvoiceDto>;

public record UpdateInvoiceItemCommand(
    Guid InvoiceId,
    Guid ItemId,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Discount,
    decimal TaxRatePercentage) : ICommand<InvoiceDto>;

public record RemoveInvoiceItemCommand(
    Guid InvoiceId,
    Guid ItemId) : ICommand<InvoiceDto>;

public record IssueInvoiceCommand(
    Guid InvoiceId) : ICommand<InvoiceDto>;

public record CancelInvoiceCommand(
    Guid InvoiceId,
    string Reason) : ICommand<InvoiceDto>;

public record DeleteInvoiceCommand(
    Guid InvoiceId) : ICommand<bool>;

public record RecordPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    string Currency,
    DateTime? PaidAt,
    string Method,
    string Reference,
    string Notes) : ICommand<PaymentDto>;

public class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IInvoiceNumberGenerator _numberGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IInvoiceNumberGenerator numberGenerator,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _numberGenerator = numberGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> HandleAsync(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoiceNumber = await _numberGenerator.GenerateInvoiceNumberAsync(cancellationToken);
        var currency = string.IsNullOrWhiteSpace(command.Currency) ? "USD" : command.Currency.ToUpperInvariant();

        var invoice = Invoice.Create(
            invoiceNumber,
            command.CustomerId,
            command.ProjectId,
            command.OpportunityId,
            command.ProposalId,
            command.IssueDate,
            command.DueDate,
            currency,
            command.Notes);

        if (command.Items != null && command.Items.Any())
        {
            foreach (var itemReq in command.Items)
            {
                invoice.AddItem(
                    itemReq.Description,
                    itemReq.Quantity,
                    Money.Create(itemReq.UnitPrice, currency),
                    Money.Create(itemReq.Discount, currency),
                    itemReq.TaxRatePercentage);
            }
        }

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.ToDto();
    }
}

public class UpdateInvoiceCommandHandler : ICommandHandler<UpdateInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> HandleAsync(UpdateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Invoice", command.InvoiceId);

        invoice.UpdateHeader(
            command.ProjectId,
            command.OpportunityId,
            command.ProposalId,
            command.IssueDate,
            command.DueDate,
            command.Notes);

        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.ToDto();
    }
}

public class AddInvoiceItemCommandHandler : ICommandHandler<AddInvoiceItemCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddInvoiceItemCommandHandler(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> HandleAsync(AddInvoiceItemCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Invoice", command.InvoiceId);

        invoice.AddItem(
            command.Description,
            command.Quantity,
            Money.Create(command.UnitPrice, invoice.Currency),
            Money.Create(command.Discount, invoice.Currency),
            command.TaxRatePercentage);

        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.ToDto();
    }
}

public class UpdateInvoiceItemCommandHandler : ICommandHandler<UpdateInvoiceItemCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateInvoiceItemCommandHandler(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> HandleAsync(UpdateInvoiceItemCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Invoice", command.InvoiceId);

        invoice.UpdateItem(
            command.ItemId,
            command.Description,
            command.Quantity,
            Money.Create(command.UnitPrice, invoice.Currency),
            Money.Create(command.Discount, invoice.Currency),
            command.TaxRatePercentage);

        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.ToDto();
    }
}

public class RemoveInvoiceItemCommandHandler : ICommandHandler<RemoveInvoiceItemCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveInvoiceItemCommandHandler(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> HandleAsync(RemoveInvoiceItemCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Invoice", command.InvoiceId);

        invoice.RemoveItem(command.ItemId);

        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.ToDto();
    }
}

public class IssueInvoiceCommandHandler : ICommandHandler<IssueInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IssueInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> HandleAsync(IssueInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Invoice", command.InvoiceId);

        invoice.Issue();

        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.ToDto();
    }
}

public class CancelInvoiceCommandHandler : ICommandHandler<CancelInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> HandleAsync(CancelInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Invoice", command.InvoiceId);

        invoice.Cancel(command.Reason);

        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoice.ToDto();
    }
}

public class DeleteInvoiceCommandHandler : ICommandHandler<DeleteInvoiceCommand, bool>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(DeleteInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Invoice", command.InvoiceId);

        invoice.SoftDelete();
        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public class RecordPaymentCommandHandler : ICommandHandler<RecordPaymentCommand, PaymentDto>
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordPaymentCommandHandler(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentDto> HandleAsync(RecordPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(command.InvoiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Invoice", command.InvoiceId);

        var paymentCurrency = string.IsNullOrWhiteSpace(command.Currency) ? invoice.Currency : command.Currency.ToUpperInvariant();
        var paymentAmount = Money.Create(command.Amount, paymentCurrency);
        var paidAt = command.PaidAt ?? DateTime.UtcNow;

        var payment = invoice.RecordPayment(
            paymentAmount,
            paidAt,
            command.Method,
            command.Reference,
            command.Notes);

        await _paymentRepository.AddAsync(payment, cancellationToken);
        _invoiceRepository.Update(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return payment.ToDto();
    }
}
