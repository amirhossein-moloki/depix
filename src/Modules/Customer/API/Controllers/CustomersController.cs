using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Application.Contracts;
using Modules.Customer.Application.Commands;
using Modules.Customer.Application.DTOs;
using Modules.Customer.Application.Queries;

namespace Modules.Customer.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICommandHandler<CreateCustomerCommand, CustomerDto> _createCustomerHandler;
    private readonly ICommandHandler<UpdateCustomerCommand, CustomerDto> _updateCustomerHandler;
    private readonly ICommandHandler<AssignCustomerCommand, CustomerDto> _assignCustomerHandler;
    private readonly ICommandHandler<ArchiveCustomerCommand> _archiveCustomerHandler;
    private readonly ICommandHandler<ReactivateCustomerCommand, CustomerDto> _reactivateCustomerHandler;

    private readonly IQueryHandler<GetCustomerByIdQuery, CustomerDetailDto> _getCustomerByIdHandler;
    private readonly IQueryHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>> _getCustomersHandler;
    private readonly IQueryHandler<GetCustomerByCompanyQuery, CustomerDetailDto> _getCustomerByCompanyHandler;
    private readonly IQueryHandler<GetCustomerContactsQuery, List<CustomerContactContractDto>> _getCustomerContactsHandler;

    private readonly IValidator<CreateCustomerCommand> _createValidator;
    private readonly IValidator<UpdateCustomerCommand> _updateValidator;
    private readonly IValidator<AssignCustomerCommand> _assignValidator;

    public CustomersController(
        ICommandHandler<CreateCustomerCommand, CustomerDto> createCustomerHandler,
        ICommandHandler<UpdateCustomerCommand, CustomerDto> updateCustomerHandler,
        ICommandHandler<AssignCustomerCommand, CustomerDto> assignCustomerHandler,
        ICommandHandler<ArchiveCustomerCommand> archiveCustomerHandler,
        ICommandHandler<ReactivateCustomerCommand, CustomerDto> reactivateCustomerHandler,
        IQueryHandler<GetCustomerByIdQuery, CustomerDetailDto> getCustomerByIdHandler,
        IQueryHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>> getCustomersHandler,
        IQueryHandler<GetCustomerByCompanyQuery, CustomerDetailDto> getCustomerByCompanyHandler,
        IQueryHandler<GetCustomerContactsQuery, List<CustomerContactContractDto>> getCustomerContactsHandler,
        IValidator<CreateCustomerCommand> createValidator,
        IValidator<UpdateCustomerCommand> updateValidator,
        IValidator<AssignCustomerCommand> assignValidator)
    {
        _createCustomerHandler = createCustomerHandler;
        _updateCustomerHandler = updateCustomerHandler;
        _assignCustomerHandler = assignCustomerHandler;
        _archiveCustomerHandler = archiveCustomerHandler;
        _reactivateCustomerHandler = reactivateCustomerHandler;
        _getCustomerByIdHandler = getCustomerByIdHandler;
        _getCustomersHandler = getCustomersHandler;
        _getCustomerByCompanyHandler = getCustomerByCompanyHandler;
        _getCustomerContactsHandler = getCustomerContactsHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _assignValidator = assignValidator;
    }

    [HttpPost]
    [Authorize(Policy = "Customer.Create")]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.CompanyId,
            request.CustomerNumber,
            request.CustomerSince,
            request.PrimaryContactId,
            request.AssignedTo,
            request.Notes
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Customer creation validation failed.", errors);
        }

        var customer = await _createCustomerHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetCustomerById), new { id = customer.Id }, ResponseFactory.Success(customer, "Customer created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "Customer.Read")]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? assignedTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomersQuery(page, pageSize, search, status, companyId, assignedTo, sortBy, sortDescending);
        var result = await _getCustomersHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Customers retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Customer.Read")]
    public async Task<IActionResult> GetCustomerById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);
        var customer = await _getCustomerByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(customer, "Customer details retrieved successfully."));
    }

    [HttpGet("company/{companyId:guid}")]
    [Authorize(Policy = "Customer.Read")]
    public async Task<IActionResult> GetCustomerByCompany(Guid companyId, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByCompanyQuery(companyId);
        var customer = await _getCustomerByCompanyHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(customer, "Customer details for company retrieved successfully."));
    }

    [HttpGet("{id:guid}/contacts")]
    [Authorize(Policy = "Customer.Read")]
    public async Task<IActionResult> GetCustomerContacts(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerContactsQuery(id);
        var contacts = await _getCustomerContactsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(contacts, "Customer contacts retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Customer.Update")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.PrimaryContactId,
            request.Notes,
            request.Status
        );

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Customer update validation failed.", errors);
        }

        var customer = await _updateCustomerHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(customer, "Customer updated successfully."));
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = "Customer.Assign")]
    public async Task<IActionResult> AssignCustomer(Guid id, [FromBody] AssignCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignCustomerCommand(id, request.AssignedTo);

        var validationResult = await _assignValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Customer assignment validation failed.", errors);
        }

        var customer = await _assignCustomerHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(customer, "Customer assigned successfully."));
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Policy = "Customer.Update")]
    public async Task<IActionResult> ReactivateCustomer(Guid id, CancellationToken cancellationToken)
    {
        var command = new ReactivateCustomerCommand(id);
        var customer = await _reactivateCustomerHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(customer, "Customer reactivated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Customer.Delete")]
    public async Task<IActionResult> ArchiveCustomer(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? archivedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new ArchiveCustomerCommand(id, archivedBy);
        await _archiveCustomerHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Customer archived successfully.", string.Empty));
    }
}
