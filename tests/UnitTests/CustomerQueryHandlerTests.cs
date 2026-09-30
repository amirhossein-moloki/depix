using BuildingBlocks.Application.Contracts;
using NSubstitute;
using Xunit;
using Modules.Customer.Application.Queries;
using Modules.Customer.Domain.Entities;
using Modules.Customer.Domain.Repositories;

namespace UnitTests;

public class CustomerQueryHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly ICustomerContactService _customerContactService = Substitute.For<ICustomerContactService>();

    [Fact]
    public async Task GetCustomerById_WhenExists_ShouldReturnDetailDto()
    {
        var companyId = Guid.NewGuid();
        var customer = Customer.Create(companyId, "CUST-1001", notes: "Test notes");
        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var handler = new GetCustomerByIdQueryHandler(_customerRepository);
        var result = await handler.HandleAsync(new GetCustomerByIdQuery(customer.Id));

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.Id);
        Assert.Equal("CUST-1001", result.CustomerNumber);
        Assert.Equal("Test notes", result.Notes);
    }

    [Fact]
    public async Task GetCustomerContacts_ShouldReturnContactsForCustomerCompany()
    {
        var companyId = Guid.NewGuid();
        var customer = Customer.Create(companyId, "CUST-1001");
        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var contacts = new List<CustomerContactContractDto>
        {
            new(Guid.NewGuid(), companyId, "Jane Doe", "CEO", "555-1234", "jane@company.com", true, "High"),
            new(Guid.NewGuid(), companyId, "John Smith", "CTO", "555-5678", "john@company.com", false, "Medium")
        };

        _customerContactService.GetContactsByCompanyIdAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(contacts);

        var handler = new GetCustomerContactsQueryHandler(_customerRepository, _customerContactService);
        var result = await handler.HandleAsync(new GetCustomerContactsQuery(customer.Id));

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Jane Doe");
        Assert.Contains(result, c => c.Name == "John Smith");
    }
}
