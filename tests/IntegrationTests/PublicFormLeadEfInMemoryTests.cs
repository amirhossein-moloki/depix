using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.CRM.Application.Features.Leads.Commands;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Infrastructure.Persistence.Repositories;
using Xunit;

namespace IntegrationTests;

public class PublicFormLeadEfInMemoryTests
{
    static PublicFormLeadEfInMemoryTests()
    {
        // Ensure CRM Assembly is initialized so OnModelCreating loads module configurations
        _ = Modules.CRM.Domain.AssemblyReference.Assembly;
    }

    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
    }

    [Fact]
    public async Task SubmitFreeConsultation_ShouldPersistCompanyContactAndLeadInDatabase()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(SubmitFreeConsultation_ShouldPersistCompanyContactAndLeadInDatabase));
        await using var dbContext = new ApplicationDbContext(options);

        var leadRepo = new LeadRepository(dbContext);
        var companyRepo = new CompanyRepository(dbContext);
        var contactRepo = new ContactRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

        var handler = new SubmitFreeConsultationCommandHandler(leadRepo, companyRepo, contactRepo, unitOfWork);
        var command = new SubmitFreeConsultationCommand("Sara Tehrani", "09121112233", "sara@example.com", "Consultation regarding ERP integration");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);

        await using var readDbContext = new ApplicationDbContext(options);
        var readLeadRepo = new LeadRepository(readDbContext);
        var readCompanyRepo = new CompanyRepository(readDbContext);
        var readContactRepo = new ContactRepository(readDbContext);

        var lead = await readLeadRepo.GetByIdAsync(result.Id);
        Assert.NotNull(lead);
        Assert.Equal("Free Consultation", lead.Source);
        Assert.Equal("Free Consultation Request - Sara Tehrani", lead.Title);

        var company = await readCompanyRepo.GetByIdAsync(lead.CompanyId);
        Assert.NotNull(company);
        Assert.Equal("Sara Tehrani", company.Name);
        Assert.Equal("09121112233", company.Phone);

        var contact = await readContactRepo.GetByIdAsync(lead.ContactId!.Value);
        Assert.NotNull(contact);
        Assert.Equal("Sara Tehrani", contact.FirstName);
        Assert.Equal("sara@example.com", contact.Email);
    }

    [Fact]
    public async Task SubmitInstagramLead_ShouldPersistCompanyContactAndLeadInDatabase()
    {
        // Arrange
        var options = CreateInMemoryOptions(nameof(SubmitInstagramLead_ShouldPersistCompanyContactAndLeadInDatabase));
        await using var dbContext = new ApplicationDbContext(options);

        var leadRepo = new LeadRepository(dbContext);
        var companyRepo = new CompanyRepository(dbContext);
        var contactRepo = new ContactRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, NSubstitute.Substitute.For<BuildingBlocks.Application.Events.IEventBus>());

        var handler = new SubmitInstagramLeadCommandHandler(leadRepo, companyRepo, contactRepo, unitOfWork);
        var command = new SubmitInstagramLeadCommand("Tehran Tech", "09124445566", "@tehrantech", "Need CRM for sales team");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);

        await using var readDbContext = new ApplicationDbContext(options);
        var readLeadRepo = new LeadRepository(readDbContext);
        var readCompanyRepo = new CompanyRepository(readDbContext);
        var readContactRepo = new ContactRepository(readDbContext);

        var lead = await readLeadRepo.GetByIdAsync(result.Id);
        Assert.NotNull(lead);
        Assert.Equal("Instagram", lead.Source);
        Assert.Equal("Instagram Lead - Tehran Tech (@tehrantech)", lead.Title);

        var company = await readCompanyRepo.GetByIdAsync(lead.CompanyId);
        Assert.NotNull(company);
        Assert.Equal("Tehran Tech", company.Name);
        Assert.Equal("https://instagram.com/tehrantech", company.Website);

        var contact = await readContactRepo.GetByIdAsync(lead.ContactId!.Value);
        Assert.NotNull(contact);
        Assert.Equal("Tehran Tech", contact.FirstName);
        Assert.Equal("09124445566", contact.Phone);
    }
}
