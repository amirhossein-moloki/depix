using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Application.Features.Leads.Commands;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.ValueObjects;
using Modules.CRM.Infrastructure.Persistence.Repositories;
using Modules.Customer.Domain.Entities;
using Modules.Customer.Infrastructure.Persistence.Repositories;
using Modules.Customer.Infrastructure.Services;
using Xunit;

namespace IntegrationTests;

public class LeadConversionIntegrationTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions(string dbName)
    {
        _ = Modules.Identity.Domain.AssemblyReference.Assembly;
        _ = Modules.Identity.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.CRM.Domain.AssemblyReference.Assembly;
        _ = Modules.CRM.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Sales.Domain.AssemblyReference.Assembly;
        _ = Modules.Sales.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Customer.Domain.AssemblyReference.Assembly;
        _ = Modules.Customer.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Project.Domain.AssemblyReference.Assembly;
        _ = Modules.Project.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Finance.Domain.AssemblyReference.Assembly;
        _ = Modules.Finance.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Support.Domain.AssemblyReference.Assembly;
        _ = Modules.Support.Infrastructure.AssemblyReference.Assembly;
        _ = Modules.Platform.Domain.AssemblyReference.Assembly;
        _ = Modules.Platform.Infrastructure.AssemblyReference.Assembly;

        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task CompleteLeadConversionLifecycle_ShouldConvertLeadAndPreserveHistoryAndPreventDuplicates()
    {
        var dbName = nameof(CompleteLeadConversionLifecycle_ShouldConvertLeadAndPreserveHistoryAndPreventDuplicates);
        var options = CreateInMemoryOptions(dbName);

        Guid companyId;
        Guid contactId;
        Guid leadId;

        // Step 1: Create Company, Contact, Lead with Activity & Sales Note
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var companyRepo = new CompanyRepository(dbContext);
            var contactRepo = new ContactRepository(dbContext);
            var leadRepo = new LeadRepository(dbContext);

            var company = Company.Create("Innovatech Corp", "Software", "https://innovatech.com", "+123456789", "contact@innovatech.com", new Address("100 Innovation Way"));
            await companyRepo.AddAsync(company);
            companyId = company.Id;

            var contact = Contact.Create(companyId, "Sarah", "Connor", "sarah@innovatech.com", "+123456789", "CTO", "Primary contact", isDecisionMaker: true, influenceLevel: "High");
            await contactRepo.AddAsync(contact);
            contactId = contact.Id;

            var lead = Lead.Create(companyId, "Enterprise License Deal", "Website", "Interested in ERP platform", 100000m, contactId);
            lead.Qualify(); // State = QUALIFIED
            await leadRepo.AddAsync(lead);
            leadId = lead.Id;

            var activity = Activity.Create(leadId, null, "CALL", "Discovery Call", "Initial Discovery Call", "Client confirmed interest in full suite", 80, contactId, companyId);
            lead.AddActivity(activity);

            var salesNote = SalesNote.Create(leadId, "High budget allocated", "High budget allocated", "Price objection handled", "Offer standard 10% discount", 80, Guid.NewGuid(), companyId: companyId, contactId: contactId);
            lead.AddSalesNote(salesNote);

            await dbContext.SaveChangesAsync();
        }

        // Step 2: Convert Lead to Customer using Handler
        await using (var dbContext = new ApplicationDbContext(options))
        {
            var leadRepo = new LeadRepository(dbContext);
            var customerRepo = new CustomerRepository(dbContext);
            var customerService = new CustomerService(customerRepo);
            var unitOfWork = new BuildingBlocks.Infrastructure.Persistence.UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new ConvertLeadCommandHandler(leadRepo, customerService, unitOfWork);
            var command = new ConvertLeadCommand(leadId);

            var result = await handler.HandleAsync(command);

            Assert.NotNull(result);
            Assert.Equal(leadId, result.LeadId);
            Assert.Equal(companyId, result.CompanyId);
            Assert.Equal(contactId, result.ContactId);
            Assert.Equal("CONVERTED", result.Status);
            Assert.NotEqual(Guid.Empty, result.CustomerId);
        }

        // Step 3: Verify Persistence, Relationships, History Preservation & Duplicate Company/Contact Check
        await using (var verifyDbContext = new ApplicationDbContext(options))
        {
            var lead = await verifyDbContext.Set<Lead>()
                .Include(l => l.Activities)
                .Include(l => l.SalesNotes)
                .FirstOrDefaultAsync(l => l.Id == leadId);

            Assert.NotNull(lead);
            Assert.Equal("CONVERTED", lead.Status);
            Assert.NotNull(lead.CustomerId);
            Assert.NotNull(lead.ConvertedAt);

            // Verify Activities & Sales Notes preserved
            Assert.Single(lead.Activities);
            Assert.Equal("Discovery Call", lead.Activities.First().Subject);
            Assert.Single(lead.SalesNotes);
            Assert.Equal("High budget allocated", lead.SalesNotes.First().NeedAnalysis);

            // Verify Customer created for company
            var customer = await verifyDbContext.Set<Customer>().FirstOrDefaultAsync(c => c.CompanyId == companyId);
            Assert.NotNull(customer);
            Assert.Equal(lead.CustomerId, customer.Id);

            // Verify Companies and Contacts were not duplicated
            var companyCount = await verifyDbContext.Set<Company>().CountAsync(c => c.Id == companyId);
            Assert.Equal(1, companyCount);

            var contactCount = await verifyDbContext.Set<Contact>().CountAsync(c => c.Id == contactId);
            Assert.Equal(1, contactCount);
        }

        // Step 4: Attempt duplicate conversion on same Lead
        await using (var duplicateDbContext = new ApplicationDbContext(options))
        {
            var leadRepo = new LeadRepository(duplicateDbContext);
            var customerRepo = new CustomerRepository(duplicateDbContext);
            var customerService = new CustomerService(customerRepo);
            var unitOfWork = new BuildingBlocks.Infrastructure.Persistence.UnitOfWork(duplicateDbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

            var handler = new ConvertLeadCommandHandler(leadRepo, customerService, unitOfWork);
            var command = new ConvertLeadCommand(leadId);

            var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => handler.HandleAsync(command));
            Assert.Contains("already been converted", ex.Message);
        }
    }
}
