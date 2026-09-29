using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Contacts.Commands;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Domain.ValueObjects;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class ContactCommandHandlerTests
{
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly ICompanyRepository _companyRepository = Substitute.For<ICompanyRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateContactHandler_ShouldAddContactAndSaveChanges_WhenCompanyExists()
    {
        // Arrange
        var company = Company.Create("Acme Corp", "Tech", "", "", "", new Address(""), "LEAD");
        _companyRepository.GetByIdAsync(company.Id, Arg.Any<CancellationToken>())
            .Returns(company);

        var handler = new CreateContactCommandHandler(_contactRepository, _companyRepository, _unitOfWork);
        var command = new CreateContactCommand(
            company.Id,
            "John",
            "Doe",
            "john.doe@acme.com",
            "+1234567890",
            "CTO",
            "Key Contact",
            true,
            "High"
        );

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("John Doe", result.FullName);
        Assert.Equal("john.doe@acme.com", result.Email);
        await _contactRepository.Received(1).AddAsync(Arg.Is<Contact>(c => c.Email == "john.doe@acme.com"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateContactHandler_ShouldThrowNotFound_WhenCompanyDoesNotExist()
    {
        // Arrange
        _companyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Company?)null);

        var handler = new CreateContactCommandHandler(_contactRepository, _companyRepository, _unitOfWork);
        var command = new CreateContactCommand(Guid.NewGuid(), "John", "Doe", "john@test.com", "", "", "");

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateContactHandler_ShouldUpdateAndSave_WhenContactExists()
    {
        // Arrange
        var existingContact = Contact.Create(Guid.NewGuid(), "OldFirst", "OldLast", "old@test.com", "", "", "");
        _contactRepository.GetByIdAsync(existingContact.Id, Arg.Any<CancellationToken>())
            .Returns(existingContact);

        var handler = new UpdateContactCommandHandler(_contactRepository, _unitOfWork);
        var command = new UpdateContactCommand(
            existingContact.Id,
            "NewFirst",
            "NewLast",
            "new@test.com",
            "99999",
            "Manager",
            "Updated Notes",
            true,
            "Medium"
        );

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("NewFirst", result.FirstName);
        Assert.Equal("NewLast", result.LastName);
        Assert.Equal("NewFirst NewLast", result.FullName);
        Assert.Equal("new@test.com", result.Email);
        _contactRepository.Received(1).Update(existingContact);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateContactHandler_ShouldThrowNotFound_WhenContactDoesNotExist()
    {
        // Arrange
        _contactRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Contact?)null);

        var handler = new UpdateContactCommandHandler(_contactRepository, _unitOfWork);
        var command = new UpdateContactCommand(Guid.NewGuid(), "First", "Last", "", "", "", "");

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task ArchiveContactHandler_ShouldSoftDeleteAndSave_WhenContactExists()
    {
        // Arrange
        var existingContact = Contact.Create(Guid.NewGuid(), "Archive", "Me", "archive@test.com", "", "", "");
        _contactRepository.GetByIdAsync(existingContact.Id, Arg.Any<CancellationToken>())
            .Returns(existingContact);

        var handler = new ArchiveContactCommandHandler(_contactRepository, _unitOfWork);
        var command = new ArchiveContactCommand(existingContact.Id, Guid.NewGuid());

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(existingContact.IsDeleted);
        _contactRepository.Received(1).Update(existingContact);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveContactHandler_ShouldThrowNotFound_WhenContactDoesNotExist()
    {
        // Arrange
        _contactRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Contact?)null);

        var handler = new ArchiveContactCommandHandler(_contactRepository, _unitOfWork);
        var command = new ArchiveContactCommand(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }
}
