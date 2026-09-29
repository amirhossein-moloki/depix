using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Contacts.Queries;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class ContactQueryHandlerTests
{
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();

    [Fact]
    public async Task GetContactByIdQueryHandler_ShouldReturnDto_WhenFound()
    {
        // Arrange
        var contact = Contact.Create(Guid.NewGuid(), "Jane", "Doe", "jane.doe@test.com", "123", "CEO", "Primary Contact", true, "High");
        _contactRepository.GetByIdAsync(contact.Id, Arg.Any<CancellationToken>())
            .Returns(contact);

        var handler = new GetContactByIdQueryHandler(_contactRepository);
        var query = new GetContactByIdQuery(contact.Id);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(contact.Id, result.Id);
        Assert.Equal("Jane", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("Jane Doe", result.FullName);
    }

    [Fact]
    public async Task GetContactByIdQueryHandler_ShouldThrowNotFound_WhenMissing()
    {
        // Arrange
        _contactRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Contact?)null);

        var handler = new GetContactByIdQueryHandler(_contactRepository);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(new GetContactByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task GetContactsQueryHandler_ShouldReturnPagedResult()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var list = new List<Contact>
        {
            Contact.Create(companyId, "Contact", "One", "one@test.com", "", "", ""),
            Contact.Create(companyId, "Contact", "Two", "two@test.com", "", "", "")
        };

        _contactRepository.GetListAsync(1, 10, companyId, null, Arg.Any<CancellationToken>())
            .Returns(list);
        _contactRepository.CountAsync(companyId, null, Arg.Any<CancellationToken>())
            .Returns(2);

        var handler = new GetContactsQueryHandler(_contactRepository);
        var query = new GetContactsQuery(1, 10, companyId);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Contact One", result.Items[0].FullName);
    }

    [Fact]
    public async Task SearchContactsQueryHandler_ShouldReturnSearchResults()
    {
        // Arrange
        var list = new List<Contact>
        {
            Contact.Create(Guid.NewGuid(), "Alice", "Search", "alice.search@test.com", "555-1234", "", "")
        };

        _contactRepository.GetListAsync(1, 10, null, "alice", Arg.Any<CancellationToken>())
            .Returns(list);
        _contactRepository.CountAsync(null, "alice", Arg.Any<CancellationToken>())
            .Returns(1);

        var handler = new SearchContactsQueryHandler(_contactRepository);
        var query = new SearchContactsQuery("alice", null, 1, 10);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Alice Search", result.Items[0].FullName);
    }
}
