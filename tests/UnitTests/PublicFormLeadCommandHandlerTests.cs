using Modules.CRM.Application.Features.Leads.Commands;
using Modules.CRM.Application.Features.Leads.Validators;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class PublicFormLeadCommandHandlerTests
{
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();
    private readonly ICompanyRepository _companyRepository = Substitute.For<ICompanyRepository>();
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly BuildingBlocks.Application.Persistence.IUnitOfWork _unitOfWork = Substitute.For<BuildingBlocks.Application.Persistence.IUnitOfWork>();

    [Fact]
    public async Task SubmitFreeConsultationHandler_WithValidData_ShouldCreateCompanyContactLeadAndReturnDto()
    {
        // Arrange
        var handler = new SubmitFreeConsultationCommandHandler(_leadRepository, _companyRepository, _contactRepository, _unitOfWork);
        var command = new SubmitFreeConsultationCommand(
            UserName: "Ali Rezaei",
            PhoneNumber: "09123456789",
            Email: "ali@example.com",
            Description: "I need consultation for software development."
        );

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Free Consultation Request - Ali Rezaei", result.Title);
        Assert.Equal("Free Consultation", result.Source);
        Assert.Equal("NEW", result.Status);

        await _companyRepository.Received(1).AddAsync(Arg.Is<Company>(c => c.Name == "Ali Rezaei" && c.Phone == "09123456789"), Arg.Any<CancellationToken>());
        await _contactRepository.Received(1).AddAsync(Arg.Is<Contact>(ct => ct.FirstName == "Ali Rezaei" && ct.Phone == "09123456789"), Arg.Any<CancellationToken>());
        await _leadRepository.Received(1).AddAsync(Arg.Is<Lead>(l => l.Source == "Free Consultation"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SubmitFreeConsultationCommandValidator_ShouldValidateFields()
    {
        // Arrange
        var validator = new SubmitFreeConsultationCommandValidator();

        var invalidCommand = new SubmitFreeConsultationCommand("", "", "invalid-email");
        var result = validator.Validate(invalidCommand);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UserName");
        Assert.Contains(result.Errors, e => e.PropertyName == "PhoneNumber");
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");

        var validCommand = new SubmitFreeConsultationCommand("Ali Rezaei", "09123456789", "ali@example.com", "Descr");
        var validResult = validator.Validate(validCommand);
        Assert.True(validResult.IsValid);
    }

    [Fact]
    public async Task SubmitInstagramLeadHandler_WithValidData_ShouldCreateCompanyContactLeadAndReturnDto()
    {
        // Arrange
        var handler = new SubmitInstagramLeadCommandHandler(_leadRepository, _companyRepository, _contactRepository, _unitOfWork);
        var command = new SubmitInstagramLeadCommand(
            BrandName: "Depix Agency",
            PhoneNumber: "09129876543",
            InstagramId: "@depix_official",
            Description: "Interested in CRM solution."
        );

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Instagram Lead - Depix Agency (@depix_official)", result.Title);
        Assert.Equal("Instagram", result.Source);
        Assert.Equal("NEW", result.Status);

        await _companyRepository.Received(1).AddAsync(Arg.Is<Company>(c => c.Name == "Depix Agency" && c.Website == "https://instagram.com/depix_official"), Arg.Any<CancellationToken>());
        await _contactRepository.Received(1).AddAsync(Arg.Is<Contact>(ct => ct.FirstName == "Depix Agency"), Arg.Any<CancellationToken>());
        await _leadRepository.Received(1).AddAsync(Arg.Is<Lead>(l => l.Source == "Instagram"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SubmitInstagramLeadCommandValidator_ShouldValidateFields()
    {
        // Arrange
        var validator = new SubmitInstagramLeadCommandValidator();

        var invalidCommand = new SubmitInstagramLeadCommand("", "", "");
        var result = validator.Validate(invalidCommand);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BrandName");
        Assert.Contains(result.Errors, e => e.PropertyName == "PhoneNumber");
        Assert.Contains(result.Errors, e => e.PropertyName == "InstagramId");

        var validCommand = new SubmitInstagramLeadCommand("Depix Agency", "09129876543", "depix_official", "Descr");
        var validResult = validator.Validate(validCommand);
        Assert.True(validResult.IsValid);
    }
}
