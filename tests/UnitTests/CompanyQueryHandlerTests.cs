using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Companies.Queries;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Domain.ValueObjects;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class CompanyQueryHandlerTests
{
    private readonly ICompanyRepository _companyRepository = Substitute.For<ICompanyRepository>();

    [Fact]
    public async Task GetCompanyByIdQueryHandler_ShouldReturnDto_WhenFound()
    {
        // Arrange
        var company = Company.Create("Target Corp", "Retail", "", "", "", new Address("Street 1"), "LEAD");
        _companyRepository.GetByIdAsync(company.Id, Arg.Any<CancellationToken>())
            .Returns(company);

        var handler = new GetCompanyByIdQueryHandler(_companyRepository);
        var query = new GetCompanyByIdQuery(company.Id);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(company.Id, result.Id);
        Assert.Equal("Target Corp", result.Name);
    }

    [Fact]
    public async Task GetCompanyByIdQueryHandler_ShouldThrowNotFound_WhenMissing()
    {
        // Arrange
        _companyRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Company?)null);

        var handler = new GetCompanyByIdQueryHandler(_companyRepository);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(new GetCompanyByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task GetCompaniesQueryHandler_ShouldReturnPagedResult()
    {
        // Arrange
        var list = new List<Company>
        {
            Company.Create("Comp 1", "Tech", "", "", "", new Address(""), "LEAD"),
            Company.Create("Comp 2", "Tech", "", "", "", new Address(""), "CUSTOMER")
        };

        _companyRepository.GetListAsync(1, 10, null, null, null, Arg.Any<CancellationToken>())
            .Returns(list);
        _companyRepository.CountAsync(null, null, null, Arg.Any<CancellationToken>())
            .Returns(2);

        var handler = new GetCompaniesQueryHandler(_companyRepository);
        var query = new GetCompaniesQuery(1, 10);

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Comp 1", result.Items[0].Name);
    }
}
