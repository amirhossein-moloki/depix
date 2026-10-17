using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Moq;
using Modules.CRM.Application.Features.Companies.Commands;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.SalesVisits.Commands;
using Modules.CRM.Application.Features.SalesVisits.Queries;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Domain.ValueObjects;
using Xunit;

namespace UnitTests;

public class SalesVisitAndGeoLocationTests
{
    private readonly Mock<ICompanyRepository> _companyRepoMock = new();
    private readonly Mock<ISalesVisitRepository> _salesVisitRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public void Company_SetLocation_ValidCoordinates_ShouldUpdateLatitudeAndLongitude()
    {
        var company = Company.Create("Acme Inc", "Tech", "https://acme.com", "123456", "info@acme.com", new Address("Tehran"));

        company.SetLocation(35.6892, 51.3890);

        Assert.Equal(35.6892, company.Latitude);
        Assert.Equal(51.3890, company.Longitude);
    }

    [Fact]
    public void Company_SetLocation_InvalidLatitude_ShouldThrowArgumentOutOfRangeException()
    {
        var company = Company.Create("Acme Inc", "Tech", "https://acme.com", "123456", "info@acme.com", new Address("Tehran"));

        Assert.Throws<ArgumentOutOfRangeException>(() => company.SetLocation(95.0, 51.3890));
    }

    [Fact]
    public void Company_SetLocation_OnlyOneCoordinateProvided_ShouldThrowArgumentException()
    {
        var company = Company.Create("Acme Inc", "Tech", "https://acme.com", "123456", "info@acme.com", new Address("Tehran"));

        Assert.Throws<ArgumentException>(() => company.SetLocation(35.6892, null));
    }

    [Fact]
    public void SalesVisit_Create_ValidFields_ShouldCreateSalesVisitEntity()
    {
        var companyId = Guid.NewGuid();
        var salespersonId = Guid.NewGuid();
        var visitedAt = DateTime.UtcNow;

        var visit = SalesVisit.Create(
            companyId,
            salespersonId,
            visitedAt,
            "Discussed enterprise contract pricing.",
            "Interested - Sent proposal",
            DateTime.UtcNow.AddDays(7),
            35.6892,
            51.3890,
            "GPS"
        );

        Assert.NotEqual(Guid.Empty, visit.Id);
        Assert.Equal(companyId, visit.CompanyId);
        Assert.Equal(salespersonId, visit.SalespersonId);
        Assert.Equal("Discussed enterprise contract pricing.", visit.ReportText);
        Assert.Equal("Interested - Sent proposal", visit.Result);
        Assert.Equal(35.6892, visit.Latitude);
        Assert.Equal(51.3890, visit.Longitude);
        Assert.Equal("GPS", visit.LocationSource);
    }

    [Fact]
    public async Task CreateSalesVisitCommandHandler_ValidCommand_ShouldSaveVisitAndReturnDto()
    {
        var companyId = Guid.NewGuid();
        var salespersonId = Guid.NewGuid();
        var company = Company.Create("Acme Inc", "Tech", "https://acme.com", "123456", "info@acme.com", new Address("Tehran"));

        _companyRepoMock.Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);

        var handler = new CreateSalesVisitCommandHandler(
            _salesVisitRepoMock.Object,
            _companyRepoMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new CreateSalesVisitCommand(
            companyId,
            salespersonId,
            DateTime.UtcNow,
            "In-person product demo",
            "Successful",
            DateTime.UtcNow.AddDays(3),
            35.6892,
            51.3890,
            "GPS"
        );

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal(companyId, result.CompanyId);
        Assert.Equal(salespersonId, result.SalespersonId);
        Assert.Equal("In-person product demo", result.ReportText);
        _salesVisitRepoMock.Verify(r => r.AddAsync(It.IsAny<SalesVisit>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSalesVisitCommandHandler_NonExistentCompany_ShouldThrowEntityNotFoundException()
    {
        var companyId = Guid.NewGuid();
        _companyRepoMock.Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        var handler = new CreateSalesVisitCommandHandler(
            _salesVisitRepoMock.Object,
            _companyRepoMock.Object,
            _unitOfWorkMock.Object
        );

        var command = new CreateSalesVisitCommand(
            companyId,
            Guid.NewGuid(),
            DateTime.UtcNow,
            "In-person product demo",
            "Successful"
        );

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task GetCompanyVisitHistoryQueryHandler_ValidCompany_ShouldReturnVisitList()
    {
        var companyId = Guid.NewGuid();
        var company = Company.Create("Acme Inc", "Tech", "https://acme.com", "123456", "info@acme.com", new Address("Tehran"));
        var visits = new List<SalesVisit>
        {
            SalesVisit.Create(companyId, Guid.NewGuid(), DateTime.UtcNow.AddDays(-2), "Visit 1", "Followup"),
            SalesVisit.Create(companyId, Guid.NewGuid(), DateTime.UtcNow.AddDays(-1), "Visit 2", "Closed")
        };

        _companyRepoMock.Setup(r => r.GetByIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(company);
        _salesVisitRepoMock.Setup(r => r.GetByCompanyIdAsync(companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(visits);

        var handler = new GetCompanyVisitHistoryQueryHandler(_salesVisitRepoMock.Object, _companyRepoMock.Object);

        var result = await handler.HandleAsync(new GetCompanyVisitHistoryQuery(companyId));

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
}
