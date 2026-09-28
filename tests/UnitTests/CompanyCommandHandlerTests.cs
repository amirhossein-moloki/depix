using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Companies.Commands;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Domain.ValueObjects;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class CompanyCommandHandlerTests
{
    private readonly ICompanyRepository _companyRepository = Substitute.For<ICompanyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateCompanyHandler_ShouldAddCompanyAndSaveChanges()
    {
        // Arrange
        var handler = new CreateCompanyCommandHandler(_companyRepository, _unitOfWork);
        var command = new CreateCompanyCommand(
            "Test Company",
            "Software",
            "https://test.com",
            "123456",
            "test@company.com",
            "123 Main St",
            "LEAD"
        );

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Company", result.Name);
        Assert.Equal("Software", result.Industry);
        Assert.Equal("123 Main St", result.Address.Text);
        await _companyRepository.Received(1).AddAsync(Arg.Is<Company>(c => c.Name == "Test Company"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCompanyHandler_ShouldUpdateAndSave_WhenCompanyExists()
    {
        // Arrange
        var existingCompany = Company.Create("Old", "OldInd", "", "", "", new Address("OldAddr"), "LEAD");
        _companyRepository.GetByIdAsync(existingCompany.Id, Arg.Any<CancellationToken>())
            .Returns(existingCompany);

        var handler = new UpdateCompanyCommandHandler(_companyRepository, _unitOfWork);
        var command = new UpdateCompanyCommand(
            existingCompany.Id,
            "Updated Name",
            "New Ind",
            "https://new.com",
            "999",
            "updated@test.com",
            "New Address",
            "CUSTOMER"
        );

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("CUSTOMER", result.Type);
        _companyRepository.Received(1).Update(existingCompany);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCompanyHandler_ShouldThrowNotFound_WhenCompanyDoesNotExist()
    {
        // Arrange
        _companyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Company?)null);

        var handler = new UpdateCompanyCommandHandler(_companyRepository, _unitOfWork);
        var command = new UpdateCompanyCommand(Guid.NewGuid(), "Name", "", "", "", "", "", "LEAD");

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ArchiveCompanyHandler_ShouldSoftDeleteAndSave_WhenCompanyExists()
    {
        // Arrange
        var existingCompany = Company.Create("Archive Me", "Tech", "", "", "", new Address(""), "LEAD");
        _companyRepository.GetByIdAsync(existingCompany.Id, Arg.Any<CancellationToken>())
            .Returns(existingCompany);

        var handler = new ArchiveCompanyCommandHandler(_companyRepository, _unitOfWork);
        var command = new ArchiveCompanyCommand(existingCompany.Id, Guid.NewGuid());

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(existingCompany.IsDeleted);
        _companyRepository.Received(1).Update(existingCompany);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
