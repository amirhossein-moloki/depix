using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.ValueObjects;
using Modules.CRM.Infrastructure.Persistence.Repositories;
using Modules.Customer.Domain.Entities;
using Modules.Customer.Infrastructure.Persistence.Repositories;
using Modules.Finance.Application.DTOs;
using Modules.Finance.Application.Features.Invoices.Commands;
using Modules.Finance.Application.Features.Invoices.Queries;
using Modules.Finance.Domain.Constants;
using Modules.Finance.Domain.Entities;
using Modules.Finance.Infrastructure.Persistence.Repositories;
using Modules.Finance.Infrastructure.Services;
using Modules.Project.Domain.Entities;
using Modules.Project.Infrastructure.Persistence.Repositories;
using Xunit;

namespace IntegrationTests;

public class FinanceIntegrationTests
{
    static FinanceIntegrationTests()
    {
        _ = Modules.Identity.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.CRM.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Sales.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Customer.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Project.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Finance.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Support.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Platform.Infrastructure.AssemblyReference.Assembly;
    }

    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task CompleteFinancialWorkflow_ShouldManageInvoiceLifecycleAndPaymentsCorrectly()
    {
        var dbName = nameof(CompleteFinancialWorkflow_ShouldManageInvoiceLifecycleAndPaymentsCorrectly);
        var options = CreateInMemoryOptions(dbName);

        Guid companyId;
        Guid customerId;
        Guid projectId;
        Guid invoiceId;

        // Step 1: Setup Company, Customer, and Project
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var companyRepo = new CompanyRepository(dbContext);
            var customerRepo = new CustomerRepository(dbContext);
            var projectRepo = new ProjectRepository(dbContext);

            var company = Company.Create("Acme Solutions", "Technology", "https://acme.com", "+123456789", "billing@acme.com", new Address("123 Business Rd"));
            await companyRepo.AddAsync(company);
            companyId = company.Id;

            var customer = Customer.Create(companyId, "CUST-2026-001");
            await customerRepo.AddAsync(customer);
            customerId = customer.Id;

            var project = Project.Create(
                customerId,
                "CRM Redesign",
                type: "WebDevelopment",
                companyId: companyId,
                description: "Full portal redesign",
                startDate: new DateOnly(2026, 10, 1),
                plannedDeliveryDate: new DateOnly(2026, 12, 31));
            await projectRepo.AddAsync(project);
            projectId = project.Id;

            await dbContext.SaveChangesAsync();
        }

        // Step 2: Create Draft Invoice with Items
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var invoiceRepo = new InvoiceRepository(dbContext);
            var numberGen = new InvoiceNumberGenerator(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new CreateInvoiceCommandHandler(invoiceRepo, numberGen, uow);
            var command = new CreateInvoiceCommand(
                customerId,
                projectId,
                null,
                null,
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 31),
                "USD",
                "Q4 Project Billing",
                new List<CreateInvoiceItemRequest>
                {
                    new("Phase 1 UI Design", 1, 10000m, 500m, 10m), // Subtotal 9500 + 10% tax = 10450
                    new("Phase 2 API Dev", 2, 5000m, 0m, 10m)      // Subtotal 10000 + 10% tax = 11000
                });

            var invoiceDto = await handler.HandleAsync(command);
            invoiceId = invoiceDto.Id;

            Assert.NotNull(invoiceDto);
            Assert.StartsWith("INV-", invoiceDto.InvoiceNumber);
            Assert.Equal(InvoiceStatus.Draft, invoiceDto.Status);
            Assert.Equal(20000m, invoiceDto.Subtotal);
            Assert.Equal(500m, invoiceDto.Discount);
            Assert.Equal(1950m, invoiceDto.Tax);
            Assert.Equal(21450m, invoiceDto.Total);
            Assert.Equal(21450m, invoiceDto.OutstandingBalance);
        }

        // Step 3: Issue Invoice
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var invoiceRepo = new InvoiceRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new IssueInvoiceCommandHandler(invoiceRepo, uow);
            var invoiceDto = await handler.HandleAsync(new IssueInvoiceCommand(invoiceId));

            Assert.Equal(InvoiceStatus.Issued, invoiceDto.Status);
        }

        // Step 4: Record Partial Payment
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var invoiceRepo = new InvoiceRepository(dbContext);
            var paymentRepo = new PaymentRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new RecordPaymentCommandHandler(invoiceRepo, paymentRepo, uow);
            var paymentDto = await handler.HandleAsync(new RecordPaymentCommand(
                invoiceId,
                10000m,
                "USD",
                DateTime.UtcNow,
                PaymentMethod.BankTransfer,
                "WIRE-98765",
                "First milestone payment"));

            Assert.Equal(10000m, paymentDto.Amount);
            Assert.Equal(PaymentMethod.BankTransfer, paymentDto.Method);
        }

        // Step 5: Verify Invoice State after Partial Payment
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var invoiceRepo = new InvoiceRepository(dbContext);
            var queryHandler = new GetInvoiceByIdQueryHandler(invoiceRepo);

            var invoiceDto = await queryHandler.HandleAsync(new GetInvoiceByIdQuery(invoiceId));

            Assert.Equal(InvoiceStatus.PartiallyPaid, invoiceDto.Status);
            Assert.Equal(10000m, invoiceDto.PaidAmount);
            Assert.Equal(11450m, invoiceDto.OutstandingBalance);
            Assert.Single(invoiceDto.Payments);
        }

        // Step 6: Record Final Payment
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var invoiceRepo = new InvoiceRepository(dbContext);
            var paymentRepo = new PaymentRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new RecordPaymentCommandHandler(invoiceRepo, paymentRepo, uow);
            await handler.HandleAsync(new RecordPaymentCommand(
                invoiceId,
                11450m,
                "USD",
                DateTime.UtcNow,
                PaymentMethod.Card,
                "CARD-1234",
                "Final balance payment"));
        }

        // Step 7: Verify Fully Paid Invoice State and Customer/Project Queries
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var invoiceRepo = new InvoiceRepository(dbContext);
            var getByIdHandler = new GetInvoiceByIdQueryHandler(invoiceRepo);
            var getCustInvoicesHandler = new GetCustomerInvoicesQueryHandler(invoiceRepo);
            var getProjInvoicesHandler = new GetProjectInvoicesQueryHandler(invoiceRepo);

            var invoiceDto = await getByIdHandler.HandleAsync(new GetInvoiceByIdQuery(invoiceId));

            Assert.Equal(InvoiceStatus.Paid, invoiceDto.Status);
            Assert.Equal(21450m, invoiceDto.PaidAmount);
            Assert.Equal(0m, invoiceDto.OutstandingBalance);
            Assert.Equal(2, invoiceDto.Payments.Count);

            var customerInvoices = await getCustInvoicesHandler.HandleAsync(new GetCustomerInvoicesQuery(customerId));
            Assert.Single(customerInvoices);
            Assert.Equal(invoiceId, customerInvoices[0].Id);

            var projectInvoices = await getProjInvoicesHandler.HandleAsync(new GetProjectInvoicesQuery(projectId));
            Assert.Single(projectInvoices);
            Assert.Equal(invoiceId, projectInvoices[0].Id);
        }

        // Step 8: Verify Overpayment is rejected
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var invoiceRepo = new InvoiceRepository(dbContext);
            var paymentRepo = new PaymentRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new RecordPaymentCommandHandler(invoiceRepo, paymentRepo, uow);

            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => handler.HandleAsync(new RecordPaymentCommand(
                invoiceId,
                100m,
                "USD",
                DateTime.UtcNow,
                PaymentMethod.Cash,
                "",
                "Extra payment")));

            Assert.Contains("exceeds outstanding balance", ex.Message);
        }
    }
}
