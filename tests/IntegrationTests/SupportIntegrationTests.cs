using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.ValueObjects;
using Modules.CRM.Infrastructure.Persistence.Repositories;
using Modules.CRM.Infrastructure.Services;
using Modules.Customer.Domain.Entities;
using Modules.Customer.Infrastructure.Persistence.Repositories;
using Modules.Customer.Infrastructure.Services;
using Modules.Project.Domain.Entities;
using Modules.Project.Infrastructure.Persistence.Repositories;
using Modules.Project.Infrastructure.Services;
using Modules.Support.Application.Commands;
using Modules.Support.Application.DTOs;
using Modules.Support.Application.Queries;
using Modules.Support.Domain.Constants;
using Modules.Support.Domain.Repositories;
using Modules.Support.Infrastructure.Persistence.Repositories;
using Modules.Support.Infrastructure.Services;
using Xunit;

namespace IntegrationTests;

public class SupportIntegrationTests
{
    static SupportIntegrationTests()
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
    public async Task CompleteSupportWorkflow_ShouldManageSupportTicketLifecycleCorrectly()
    {
        var dbName = nameof(CompleteSupportWorkflow_ShouldManageSupportTicketLifecycleCorrectly);
        var options = CreateInMemoryOptions(dbName);

        Guid companyId;
        Guid contactId;
        Guid customerId;
        Guid projectId;
        Guid ticketId;
        Guid assignedUserId = Guid.NewGuid();

        // 1. Create Company
        // 2. Create Contact
        // 3. Create Customer
        // 4. Create Project
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var companyRepo = new CompanyRepository(dbContext);
            var contactRepo = new ContactRepository(dbContext);
            var customerRepo = new CustomerRepository(dbContext);
            var projectRepo = new ProjectRepository(dbContext);

            var company = Company.Create("TechCorp Inc", "Technology", "https://techcorp.com", "+1122334455", "support@techcorp.com", new Address("456 Innovation Way"));
            await companyRepo.AddAsync(company);
            companyId = company.Id;

            var contact = Contact.Create(companyId, "Alice", "Smith", "alice@techcorp.com", "+1122334456", "CTO", "Primary technical contact");
            await contactRepo.AddAsync(contact);
            contactId = contact.Id;

            var customer = Customer.Create(companyId, "CUST-2026-999");
            await customerRepo.AddAsync(customer);
            customerId = customer.Id;

            var project = Project.Create(
                customerId,
                "Cloud Migration Phase 2",
                type: "DevOps",
                companyId: companyId,
                description: "Migrating core services to cloud",
                startDate: new DateOnly(2026, 1, 1),
                plannedDeliveryDate: new DateOnly(2026, 6, 30));
            await projectRepo.AddAsync(project);
            projectId = project.Id;

            await dbContext.SaveChangesAsync();
        }

        // 5. Create Ticket
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var numberGen = new TicketNumberGenerator(dbContext);
            var custService = new CustomerService(new CustomerRepository(dbContext));
            var projService = new ProjectService(new ProjectRepository(dbContext));
            var contactService = new CustomerContactService(new ContactRepository(dbContext));
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new CreateTicketCommandHandler(
                ticketRepo,
                numberGen,
                custService,
                projService,
                contactService,
                uow);

            var command = new CreateTicketCommand(
                customerId,
                projectId,
                contactId,
                "API Endpoint Timeout",
                "POST /api/sales/proposals timing out under load",
                TicketPriority.Normal,
                TicketCategory.Bug,
                null,
                DateTime.UtcNow.AddDays(2));

            var ticketDto = await handler.HandleAsync(command);
            ticketId = ticketDto.Id;

            Assert.NotNull(ticketDto);
            Assert.StartsWith("TICK-", ticketDto.TicketNumber);
            Assert.Equal("API Endpoint Timeout", ticketDto.Subject);
            Assert.Equal(TicketStatus.Open, ticketDto.Status);
            Assert.Equal(TicketPriority.Normal, ticketDto.Priority);
            Assert.Equal(TicketCategory.Bug, ticketDto.Category);
            Assert.Equal(customerId, ticketDto.CustomerId);
            Assert.Equal(projectId, ticketDto.ProjectId);
            Assert.Equal(contactId, ticketDto.ContactId);
        }

        // 6. Retrieve Ticket
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var handler = new GetTicketByIdQueryHandler(ticketRepo);

            var ticketDto = await handler.HandleAsync(new GetTicketByIdQuery(ticketId));

            Assert.Equal("API Endpoint Timeout", ticketDto.Subject);
            Assert.Equal(TicketStatus.Open, ticketDto.Status);
        }

        // 7. Update Ticket
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var projService = new ProjectService(new ProjectRepository(dbContext));
            var contactService = new CustomerContactService(new ContactRepository(dbContext));
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new UpdateTicketCommandHandler(ticketRepo, projService, contactService, uow);

            var command = new UpdateTicketCommand(
                ticketId,
                "API Endpoint High Latency & Timeout",
                "POST /api/sales/proposals timing out under 500 RPS load",
                TicketCategory.TechnicalIssue,
                projectId,
                contactId,
                DateTime.UtcNow.AddDays(1));

            var updatedDto = await handler.HandleAsync(command);

            Assert.Equal("API Endpoint High Latency & Timeout", updatedDto.Subject);
            Assert.Equal(TicketCategory.TechnicalIssue, updatedDto.Category);
        }

        // 8. Assign Ticket
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new AssignTicketCommandHandler(ticketRepo, uow);

            var assignedDto = await handler.HandleAsync(new AssignTicketCommand(ticketId, assignedUserId, "Lead Support Agent"));

            Assert.Equal(assignedUserId, assignedDto.AssignedToUserId);
        }

        // 9. Change Priority
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new ChangeTicketPriorityCommandHandler(ticketRepo, uow);

            var updatedDto = await handler.HandleAsync(new ChangeTicketPriorityCommand(ticketId, TicketPriority.Urgent, "Lead Support Agent"));

            Assert.Equal(TicketPriority.Urgent, updatedDto.Priority);
        }

        // 10. Add Ticket Comment
        // 11. Verify history
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new AddTicketCommentCommandHandler(ticketRepo, uow);

            var commentDto = await handler.HandleAsync(new AddTicketCommentCommand(ticketId, assignedUserId, "DevOps Team", "Investigating DB connection pool limits."));

            Assert.NotNull(commentDto);
            Assert.Equal("Investigating DB connection pool limits.", commentDto.Message);
        }

        // 12. Move Ticket to InProgress
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new ChangeTicketStatusCommandHandler(ticketRepo, uow);

            var updatedDto = await handler.HandleAsync(new ChangeTicketStatusCommand(ticketId, TicketStatus.InProgress, "DevOps Team"));

            Assert.Equal(TicketStatus.InProgress, updatedDto.Status);
        }

        // 13. Resolve Ticket
        // 14. Verify resolution and timestamp
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new ResolveTicketCommandHandler(ticketRepo, uow);

            var resolvedDto = await handler.HandleAsync(new ResolveTicketCommand(ticketId, "Increased DB pool size to 100 and added Redis cache.", DateTime.UtcNow, "DevOps Team"));

            Assert.Equal(TicketStatus.Resolved, resolvedDto.Status);
            Assert.NotNull(resolvedDto.ResolvedAt);
            Assert.Equal("Increased DB pool size to 100 and added Redis cache.", resolvedDto.Resolution);
        }

        // 15. Close Ticket
        // 16. Verify final state
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new CloseTicketCommandHandler(ticketRepo, uow);

            var closedDto = await handler.HandleAsync(new CloseTicketCommand(ticketId, "Support Agent"));

            Assert.Equal(TicketStatus.Closed, closedDto.Status);
            Assert.NotNull(closedDto.ClosedAt);
        }

        // 17. Search/filter tickets
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var handler = new GetTicketsQueryHandler(ticketRepo);

            var filter = new TicketFilterParams(
                CustomerId: customerId,
                SearchTerm: "Latency",
                Status: TicketStatus.Closed);

            var pagedResult = await handler.HandleAsync(new GetTicketsQuery(filter));

            Assert.Single(pagedResult.Items);
            Assert.Equal(ticketId, pagedResult.Items[0].Id);
        }

        // 18. Retrieve Customer tickets
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var handler = new GetCustomerTicketsQueryHandler(ticketRepo);

            var customerTickets = await handler.HandleAsync(new GetCustomerTicketsQuery(customerId));

            Assert.Single(customerTickets);
            Assert.Equal(ticketId, customerTickets[0].Id);
        }

        // 19. Retrieve Project tickets
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var handler = new GetProjectTicketsQueryHandler(ticketRepo);

            var projectTickets = await handler.HandleAsync(new GetProjectTicketsQuery(projectId));

            Assert.Single(projectTickets);
            Assert.Equal(ticketId, projectTickets[0].Id);
        }

        // 20. Verify invalid Customer/Project/Contact relationships are rejected
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var numberGen = new TicketNumberGenerator(dbContext);
            var custService = new CustomerService(new CustomerRepository(dbContext));
            var projService = new ProjectService(new ProjectRepository(dbContext));
            var contactService = new CustomerContactService(new ContactRepository(dbContext));
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new CreateTicketCommandHandler(ticketRepo, numberGen, custService, projService, contactService, uow);

            var invalidCustomerCmd = new CreateTicketCommand(Guid.NewGuid(), null, null, "Test", "Desc", TicketPriority.Normal, TicketCategory.General, null, null);
            await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(invalidCustomerCmd));

            var invalidProjectCmd = new CreateTicketCommand(customerId, Guid.NewGuid(), null, "Test", "Desc", TicketPriority.Normal, TicketCategory.General, null, null);
            await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(invalidProjectCmd));

            var invalidContactCmd = new CreateTicketCommand(customerId, null, Guid.NewGuid(), "Test", "Desc", TicketPriority.Normal, TicketCategory.General, null, null);
            await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(invalidContactCmd));
        }

        // 21. Verify invalid state transitions are rejected
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var resolveHandler = new ResolveTicketCommandHandler(ticketRepo, uow);

            // Ticket is currently CLOSED. Attempting to resolve should fail.
            await Assert.ThrowsAsync<BusinessRuleException>(() => resolveHandler.HandleAsync(new ResolveTicketCommand(ticketId, "Resolution attempt on closed ticket")));
        }

        // 22. Verify soft-delete/audit behavior where applicable
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var uow = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var deleteHandler = new DeleteTicketCommandHandler(ticketRepo, uow);
            var deleted = await deleteHandler.HandleAsync(new DeleteTicketCommand(ticketId));

            Assert.True(deleted);
        }

        // Confirm query filter hides soft-deleted ticket
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var ticketRepo = new TicketRepository(dbContext);
            var getByIdHandler = new GetTicketByIdQueryHandler(ticketRepo);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => getByIdHandler.HandleAsync(new GetTicketByIdQuery(ticketId)));
        }
    }
}
