using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using NSubstitute;
using Xunit;
using Modules.Customer.Application.Commands;
using Modules.Customer.Application.Validators;
using Modules.Customer.Domain.Entities;
using Modules.Customer.Domain.Repositories;

namespace UnitTests;

public class CustomerCommandHandlerTests
{
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly ICustomerContactService _customerContactService = Substitute.For<ICustomerContactService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateCustomer_WhenValid_ShouldSaveAndReturnDto()
    {
        var companyId = Guid.NewGuid();
        var command = new CreateCustomerCommand(companyId, "CUST-1001", null, null, null, "Test notes");
        var handler = new CreateCustomerCommandHandler(_customerRepository, _unitOfWork);

        _customerRepository.ExistsForCompanyAsync(companyId, Arg.Any<CancellationToken>()).Returns(false);
        _customerRepository.GetByCustomerNumberAsync("CUST-1001", Arg.Any<CancellationToken>()).Returns((Customer?)null);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal(companyId, result.CompanyId);
        Assert.Equal("CUST-1001", result.CustomerNumber);
        Assert.Equal("Test notes", result.Notes);
        await _customerRepository.Received(1).AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCustomer_WhenCompanyAlreadyHasCustomer_ShouldThrowBusinessRuleException()
    {
        var companyId = Guid.NewGuid();
        var command = new CreateCustomerCommand(companyId, "CUST-1001", null, null, null, null);
        var handler = new CreateCustomerCommandHandler(_customerRepository, _unitOfWork);

        _customerRepository.ExistsForCompanyAsync(companyId, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<BusinessRuleException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task UpdateCustomer_WhenValid_ShouldUpdateFieldsAndSave()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-1001");
        var command = new UpdateCustomerCommand(customer.Id, null, "Updated notes", "INACTIVE");
        var handler = new UpdateCustomerCommandHandler(_customerRepository, _unitOfWork);

        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var result = await handler.HandleAsync(command);

        Assert.Equal("Updated notes", result.Notes);
        Assert.Equal("INACTIVE", result.Status);
        _customerRepository.Received(1).Update(customer);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangeCustomerStatus_WhenValid_ShouldUpdateStatusAndSave()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-1001");
        var command = new ChangeCustomerStatusCommand(customer.Id, "SUSPENDED");
        var handler = new ChangeCustomerStatusCommandHandler(_customerRepository, _unitOfWork);

        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var result = await handler.HandleAsync(command);

        Assert.Equal("SUSPENDED", result.Status);
        _customerRepository.Received(1).Update(customer);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetPrimaryContact_WhenValid_ShouldUpdatePrimaryContactAndSave()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-1001");
        var newContactId = Guid.NewGuid();
        var command = new SetPrimaryContactCommand(customer.Id, newContactId);
        var handler = new SetPrimaryContactCommandHandler(_customerRepository, _unitOfWork);

        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var result = await handler.HandleAsync(command);

        Assert.Equal(newContactId, result.PrimaryContactId);
        _customerRepository.Received(1).Update(customer);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignAccountManager_WhenValid_ShouldAssignManagerAndSave()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-1001");
        var accountManagerId = Guid.NewGuid();
        var command = new AssignAccountManagerCommand(customer.Id, accountManagerId);
        var handler = new AssignAccountManagerCommandHandler(_customerRepository, _unitOfWork);

        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var result = await handler.HandleAsync(command);

        Assert.Equal(accountManagerId, result.AssignedTo);
        _customerRepository.Received(1).Update(customer);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignCustomer_WhenValid_ShouldAssignUser()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-1001");
        var userId = Guid.NewGuid();
        var command = new AssignCustomerCommand(customer.Id, userId);
        var handler = new AssignCustomerCommandHandler(_customerRepository, _unitOfWork);

        _customerRepository.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        var result = await handler.HandleAsync(command);

        Assert.Equal(userId, result.AssignedTo);
        _customerRepository.Received(1).Update(customer);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCustomerValidator_WhenCrossCompanyContact_ShouldFailValidation()
    {
        var customerCompanyId = Guid.NewGuid();
        var otherCompanyId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        var contact = new CustomerContactContractDto(contactId, otherCompanyId, "John Doe", "Manager", "123456", "john@other.com", true, "High");
        _customerContactService.GetContactByIdAsync(contactId, Arg.Any<CancellationToken>()).Returns(contact);

        var validator = new CreateCustomerCommandValidator(_customerContactService);
        var command = new CreateCustomerCommand(customerCompanyId, "CUST-1001", null, contactId, null, null);

        var validationResult = await validator.ValidateAsync(command);

        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.PropertyName == "PrimaryContactId");
    }
}
