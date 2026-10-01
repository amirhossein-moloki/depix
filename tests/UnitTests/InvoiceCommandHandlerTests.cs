using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Finance.Application.DTOs;
using Modules.Finance.Application.Features.Invoices.Commands;
using Modules.Finance.Application.Features.Invoices.Queries;
using Modules.Finance.Application.Services;
using Modules.Finance.Domain.Constants;
using Modules.Finance.Domain.Entities;
using Modules.Finance.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class InvoiceCommandHandlerTests
{
    private readonly IInvoiceRepository _invoiceRepository = Substitute.For<IInvoiceRepository>();
    private readonly IPaymentRepository _paymentRepository = Substitute.For<IPaymentRepository>();
    private readonly IInvoiceNumberGenerator _numberGenerator = Substitute.For<IInvoiceNumberGenerator>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateInvoiceCommandHandler_ShouldCreateInvoiceWithGeneratedNumberAndItems()
    {
        // Arrange
        _numberGenerator.GenerateInvoiceNumberAsync(Arg.Any<CancellationToken>())
            .Returns("INV-202610-0001");

        var handler = new CreateInvoiceCommandHandler(_invoiceRepository, _numberGenerator, _unitOfWork);
        var customerId = Guid.NewGuid();

        var items = new List<CreateInvoiceItemRequest>
        {
            new("Consulting", 10, 200m, 0m, 10m)
        };

        var command = new CreateInvoiceCommand(
            customerId,
            null,
            null,
            null,
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 31),
            "USD",
            "Initial invoice",
            items);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("INV-202610-0001", result.InvoiceNumber);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal(2200m, result.Total); // 10 * 200 = 2000 + 10% tax = 2200
        await _invoiceRepository.Received(1).AddAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IssueInvoiceCommandHandler_ShouldIssueDraftInvoice()
    {
        // Arrange
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(500m, "USD"));

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var handler = new IssueInvoiceCommandHandler(_invoiceRepository, _unitOfWork);
        var command = new IssueInvoiceCommand(invoice.Id);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(InvoiceStatus.Issued, result.Status);
        _invoiceRepository.Received(1).Update(invoice);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordPaymentCommandHandler_ShouldRecordPaymentAndReturnPaymentDto()
    {
        // Arrange
        var invoice = Invoice.Create("INV-001", Guid.NewGuid());
        invoice.AddItem("Service", 1, Money.Create(1000m, "USD"));
        invoice.Issue();

        _invoiceRepository.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>())
            .Returns(invoice);

        var handler = new RecordPaymentCommandHandler(_invoiceRepository, _paymentRepository, _unitOfWork);
        var command = new RecordPaymentCommand(
            invoice.Id,
            400m,
            "USD",
            DateTime.UtcNow,
            PaymentMethod.BankTransfer,
            "REF-001",
            "Partial payment");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400m, result.Amount);
        Assert.Equal(PaymentMethod.BankTransfer, result.Method);
        await _paymentRepository.Received(1).AddAsync(Arg.Any<Payment>(), Arg.Any<CancellationToken>());
        _invoiceRepository.Received(1).Update(invoice);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetInvoiceByIdQueryHandler_WhenNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        _invoiceRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Invoice?)null);

        var handler = new GetInvoiceByIdQueryHandler(_invoiceRepository);
        var query = new GetInvoiceByIdQuery(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(query));
    }
}
