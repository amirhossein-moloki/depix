using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.ValueObjects;
using Xunit;

namespace UnitTests;

public class CompanyDomainTests
{
    [Fact]
    public void Create_ShouldInitializeCompanyCorrectly()
    {
        // Arrange & Act
        var company = Company.Create(
            "Acme Corp",
            "Technology",
            "https://acme.com",
            "+1234567890",
            "info@acme.com",
            new Address("123 Tech Lane"),
            "LEAD"
        );

        // Assert
        Assert.NotEqual(Guid.Empty, company.Id);
        Assert.Equal("Acme Corp", company.Name);
        Assert.Equal("Technology", company.Industry);
        Assert.Equal("https://acme.com", company.Website);
        Assert.Equal("+1234567890", company.Phone);
        Assert.Equal("info@acme.com", company.Email);
        Assert.Equal("123 Tech Lane", company.Address.Text);
        Assert.Equal("LEAD", company.Type);
        Assert.False(company.IsDeleted);
        Assert.Null(company.DeletedAt);
    }

    [Fact]
    public void UpdateInfo_ShouldUpdateFieldsAndTimestamp()
    {
        // Arrange
        var company = Company.Create(
            "Old Name",
            "Old Industry",
            "https://old.com",
            "11111",
            "old@test.com",
            new Address("Old Street"),
            "LEAD"
        );

        // Act
        company.UpdateInfo(
            "New Name",
            "New Industry",
            "https://new.com",
            "22222",
            "new@test.com",
            new Address("New Street"),
            "CUSTOMER"
        );

        // Assert
        Assert.Equal("New Name", company.Name);
        Assert.Equal("New Industry", company.Industry);
        Assert.Equal("https://new.com", company.Website);
        Assert.Equal("22222", company.Phone);
        Assert.Equal("new@test.com", company.Email);
        Assert.Equal("New Street", company.Address.Text);
        Assert.Equal("CUSTOMER", company.Type);
        Assert.NotNull(company.UpdatedAt);
    }

    [Fact]
    public void SoftDelete_ShouldSetIsDeletedAndAuditFields()
    {
        // Arrange
        var company = Company.Create("Delete Test", "Tech", "", "", "", new Address(""), "LEAD");
        var userId = Guid.NewGuid();

        // Act
        company.SoftDelete(userId);

        // Assert
        Assert.True(company.IsDeleted);
        Assert.NotNull(company.DeletedAt);
        Assert.Equal(userId, company.DeletedBy);
    }
}
