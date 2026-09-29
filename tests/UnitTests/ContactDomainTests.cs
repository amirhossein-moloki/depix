using Modules.CRM.Domain.Entities;
using Xunit;

namespace UnitTests;

public class ContactDomainTests
{
    [Fact]
    public void Create_ShouldInitializeContactCorrectly()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        // Act
        var contact = Contact.Create(
            companyId,
            "John",
            "Doe",
            "john.doe@example.com",
            "+1234567890",
            "CTO",
            "Technical Decision Maker",
            isDecisionMaker: true,
            influenceLevel: "High"
        );

        // Assert
        Assert.NotEqual(Guid.Empty, contact.Id);
        Assert.Equal(companyId, contact.CompanyId);
        Assert.Equal("John", contact.FirstName);
        Assert.Equal("Doe", contact.LastName);
        Assert.Equal("John Doe", contact.Name);
        Assert.Equal("john.doe@example.com", contact.Email);
        Assert.Equal("+1234567890", contact.Phone);
        Assert.Equal("CTO", contact.Position);
        Assert.Equal("Technical Decision Maker", contact.Description);
        Assert.True(contact.IsDecisionMaker);
        Assert.Equal("High", contact.InfluenceLevel);
        Assert.False(contact.IsDeleted);
        Assert.Null(contact.DeletedAt);
    }

    [Fact]
    public void UpdateInformation_ShouldUpdateContactFieldsAndTimestamp()
    {
        // Arrange
        var contact = Contact.Create(
            Guid.NewGuid(),
            "John",
            "Doe",
            "john@example.com",
            "11111",
            "Dev",
            "Desc",
            false,
            "Low"
        );

        // Act
        contact.UpdateInformation(
            "Jane",
            "Smith",
            "jane.smith@example.com",
            "22222",
            "VP Engineering",
            "Updated Desc",
            true,
            "High"
        );

        // Assert
        Assert.Equal("Jane", contact.FirstName);
        Assert.Equal("Smith", contact.LastName);
        Assert.Equal("Jane Smith", contact.Name);
        Assert.Equal("jane.smith@example.com", contact.Email);
        Assert.Equal("22222", contact.Phone);
        Assert.Equal("VP Engineering", contact.Position);
        Assert.Equal("Updated Desc", contact.Description);
        Assert.True(contact.IsDecisionMaker);
        Assert.Equal("High", contact.InfluenceLevel);
        Assert.NotNull(contact.UpdatedAt);
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedAndAuditFields()
    {
        // Arrange
        var contact = Contact.Create(Guid.NewGuid(), "Alice", "Wonderland", "alice@example.com", "", "", "");
        var userId = Guid.NewGuid();

        // Act
        contact.SoftDelete(userId);

        // Assert
        Assert.True(contact.IsDeleted);
        Assert.NotNull(contact.DeletedAt);
        Assert.Equal(userId, contact.DeletedBy);
    }

    [Fact]
    public void UndoSoftDelete_ShouldResetIsDeletedFields()
    {
        // Arrange
        var contact = Contact.Create(Guid.NewGuid(), "Bob", "Marley", "bob@example.com", "", "", "");
        contact.SoftDelete(Guid.NewGuid());

        // Act
        contact.UndoSoftDelete();

        // Assert
        Assert.False(contact.IsDeleted);
        Assert.Null(contact.DeletedAt);
        Assert.Null(contact.DeletedBy);
    }
}
