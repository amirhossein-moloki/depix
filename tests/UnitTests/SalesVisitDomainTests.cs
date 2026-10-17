using Modules.CRM.Domain.Entities;
using Xunit;

namespace UnitTests;

public class SalesVisitDomainTests
{
    [Fact]
    public void Create_ValidParameters_ShouldCreateSalesVisit()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var salespersonId = Guid.NewGuid();
        var visitDate = DateTime.UtcNow;

        // Act
        var visit = SalesVisit.Create(
            companyId,
            salespersonId,
            visitDate,
            "Discussed software licenses and renewal terms.",
            "SUCCESSFUL",
            latitude: 35.6892,
            longitude: 51.3890,
            locationSource: "GPS"
        );

        // Assert
        Assert.NotNull(visit);
        Assert.Equal(companyId, visit.CompanyId);
        Assert.Equal(salespersonId, visit.SalespersonId);
        Assert.Equal("Discussed software licenses and renewal terms.", visit.Report);
        Assert.Equal("SUCCESSFUL", visit.Result);
        Assert.Equal(35.6892, visit.Latitude);
        Assert.Equal(51.3890, visit.Longitude);
        Assert.Equal("GPS", visit.LocationSource);
    }

    [Fact]
    public void SetLocation_InvalidCoordinates_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var salespersonId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SalesVisit.Create(
                companyId,
                salespersonId,
                DateTime.UtcNow,
                "Report text",
                "SUCCESSFUL",
                latitude: 100.0, // Invalid latitude > 90
                longitude: 51.3890
            )
        );

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SalesVisit.Create(
                companyId,
                salespersonId,
                DateTime.UtcNow,
                "Report text",
                "SUCCESSFUL",
                latitude: 35.6892,
                longitude: 200.0 // Invalid longitude > 180
            )
        );
    }

    [Fact]
    public void SetLocation_PartialCoordinates_ShouldThrowArgumentException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var salespersonId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            SalesVisit.Create(
                companyId,
                salespersonId,
                DateTime.UtcNow,
                "Report text",
                "SUCCESSFUL",
                latitude: 35.6892,
                longitude: null
            )
        );
    }
}
