using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Modules.Finance.Application.DTOs;
using Modules.Finance.Application.Features.Invoices.Commands;
using Modules.Finance.Application.Features.Invoices.Queries;
using Modules.Finance.Domain.Repositories;

namespace Modules.Finance.API.Controllers;

[ApiController]
[Route("api/finance/invoices")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly ICommandHandler<CreateInvoiceCommand, InvoiceDto> _createInvoiceHandler;
    private readonly ICommandHandler<UpdateInvoiceCommand, InvoiceDto> _updateInvoiceHandler;
    private readonly ICommandHandler<AddInvoiceItemCommand, InvoiceDto> _addInvoiceItemHandler;
    private readonly ICommandHandler<UpdateInvoiceItemCommand, InvoiceDto> _updateInvoiceItemHandler;
    private readonly ICommandHandler<RemoveInvoiceItemCommand, InvoiceDto> _removeInvoiceItemHandler;
    private readonly ICommandHandler<IssueInvoiceCommand, InvoiceDto> _issueInvoiceHandler;
    private readonly ICommandHandler<CancelInvoiceCommand, InvoiceDto> _cancelInvoiceHandler;
    private readonly ICommandHandler<DeleteInvoiceCommand, bool> _deleteInvoiceHandler;
    private readonly ICommandHandler<RecordPaymentCommand, PaymentDto> _recordPaymentHandler;

    private readonly IQueryHandler<GetInvoiceByIdQuery, InvoiceDto> _getInvoiceByIdHandler;
    private readonly IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItemDto>> _getInvoicesHandler;
    private readonly IQueryHandler<GetCustomerInvoicesQuery, List<InvoiceListItemDto>> _getCustomerInvoicesHandler;
    private readonly IQueryHandler<GetProjectInvoicesQuery, List<InvoiceListItemDto>> _getProjectInvoicesHandler;
    private readonly IQueryHandler<GetInvoicePaymentsQuery, List<PaymentDto>> _getInvoicePaymentsHandler;

    public InvoicesController(
        ICommandHandler<CreateInvoiceCommand, InvoiceDto> createInvoiceHandler,
        ICommandHandler<UpdateInvoiceCommand, InvoiceDto> updateInvoiceHandler,
        ICommandHandler<AddInvoiceItemCommand, InvoiceDto> addInvoiceItemHandler,
        ICommandHandler<UpdateInvoiceItemCommand, InvoiceDto> updateInvoiceItemHandler,
        ICommandHandler<RemoveInvoiceItemCommand, InvoiceDto> removeInvoiceItemHandler,
        ICommandHandler<IssueInvoiceCommand, InvoiceDto> issueInvoiceHandler,
        ICommandHandler<CancelInvoiceCommand, InvoiceDto> cancelInvoiceHandler,
        ICommandHandler<DeleteInvoiceCommand, bool> deleteInvoiceHandler,
        ICommandHandler<RecordPaymentCommand, PaymentDto> recordPaymentHandler,
        IQueryHandler<GetInvoiceByIdQuery, InvoiceDto> getInvoiceByIdHandler,
        IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItemDto>> getInvoicesHandler,
        IQueryHandler<GetCustomerInvoicesQuery, List<InvoiceListItemDto>> getCustomerInvoicesHandler,
        IQueryHandler<GetProjectInvoicesQuery, List<InvoiceListItemDto>> getProjectInvoicesHandler,
        IQueryHandler<GetInvoicePaymentsQuery, List<PaymentDto>> getInvoicePaymentsHandler)
    {
        _createInvoiceHandler = createInvoiceHandler;
        _updateInvoiceHandler = updateInvoiceHandler;
        _addInvoiceItemHandler = addInvoiceItemHandler;
        _updateInvoiceItemHandler = updateInvoiceItemHandler;
        _removeInvoiceItemHandler = removeInvoiceItemHandler;
        _issueInvoiceHandler = issueInvoiceHandler;
        _cancelInvoiceHandler = cancelInvoiceHandler;
        _deleteInvoiceHandler = deleteInvoiceHandler;
        _recordPaymentHandler = recordPaymentHandler;
        _getInvoiceByIdHandler = getInvoiceByIdHandler;
        _getInvoicesHandler = getInvoicesHandler;
        _getCustomerInvoicesHandler = getCustomerInvoicesHandler;
        _getProjectInvoicesHandler = getProjectInvoicesHandler;
        _getInvoicePaymentsHandler = getInvoicePaymentsHandler;
    }

    [HttpPost]
    [Authorize(Policy = "Finance.Invoice.Create")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateInvoice(
        [FromBody] CreateInvoiceRequest request,
        [FromServices] IValidator<CreateInvoiceCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand(
            request.CustomerId,
            request.ProjectId,
            request.OpportunityId,
            request.ProposalId,
            request.IssueDate,
            request.DueDate,
            request.Currency,
            request.Notes,
            request.Items);

        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _createInvoiceHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetInvoiceById), new { id = result.Id }, ResponseFactory.Success(result, "Invoice created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "Finance.Invoice.Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateOnly? fromIssueDate = null,
        [FromQuery] DateOnly? toIssueDate = null,
        [FromQuery] DateOnly? fromDueDate = null,
        [FromQuery] DateOnly? toDueDate = null,
        [FromQuery] bool? isOverdue = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var filterParams = new InvoiceFilterParams(
            page,
            pageSize,
            customerId,
            projectId,
            status,
            fromIssueDate,
            toIssueDate,
            fromDueDate,
            toDueDate,
            isOverdue,
            searchTerm);

        var query = new GetInvoicesQuery(filterParams);
        var result = await _getInvoicesHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Finance.Invoice.Read")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoiceById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetInvoiceByIdQuery(id);
        var result = await _getInvoiceByIdHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result));
    }

    [HttpGet("customer/{customerId:guid}")]
    [Authorize(Policy = "Finance.Invoice.Read")]
    [ProducesResponseType(typeof(ApiResponse<List<InvoiceListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerInvoices([FromRoute] Guid customerId, CancellationToken cancellationToken)
    {
        var query = new GetCustomerInvoicesQuery(customerId);
        var result = await _getCustomerInvoicesHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result));
    }

    [HttpGet("project/{projectId:guid}")]
    [Authorize(Policy = "Finance.Invoice.Read")]
    [ProducesResponseType(typeof(ApiResponse<List<InvoiceListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectInvoices([FromRoute] Guid projectId, CancellationToken cancellationToken)
    {
        var query = new GetProjectInvoicesQuery(projectId);
        var result = await _getProjectInvoicesHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Finance.Invoice.Update")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInvoice(
        [FromRoute] Guid id,
        [FromBody] UpdateInvoiceRequest request,
        [FromServices] IValidator<UpdateInvoiceCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateInvoiceCommand(
            id,
            request.ProjectId,
            request.OpportunityId,
            request.ProposalId,
            request.IssueDate,
            request.DueDate,
            request.Notes);

        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _updateInvoiceHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Invoice updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Finance.Invoice.Delete")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteInvoice([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteInvoiceCommand(id);
        var result = await _deleteInvoiceHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Invoice deleted successfully."));
    }

    [HttpPost("{id:guid}/items")]
    [Authorize(Policy = "Finance.Invoice.Update")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddInvoiceItem(
        [FromRoute] Guid id,
        [FromBody] CreateInvoiceItemRequest request,
        [FromServices] IValidator<AddInvoiceItemCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new AddInvoiceItemCommand(
            id,
            request.Description,
            request.Quantity,
            request.UnitPrice,
            request.Discount,
            request.TaxRatePercentage);

        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _addInvoiceItemHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Invoice item added successfully."));
    }

    [HttpPut("{id:guid}/items/{itemId:guid}")]
    [Authorize(Policy = "Finance.Invoice.Update")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateInvoiceItem(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        [FromBody] UpdateInvoiceItemRequest request,
        [FromServices] IValidator<UpdateInvoiceItemCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateInvoiceItemCommand(
            id,
            itemId,
            request.Description,
            request.Quantity,
            request.UnitPrice,
            request.Discount,
            request.TaxRatePercentage);

        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _updateInvoiceItemHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Invoice item updated successfully."));
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [Authorize(Policy = "Finance.Invoice.Update")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveInvoiceItem(
        [FromRoute] Guid id,
        [FromRoute] Guid itemId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveInvoiceItemCommand(id, itemId);
        var result = await _removeInvoiceItemHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Invoice item removed successfully."));
    }

    [HttpPost("{id:guid}/issue")]
    [Authorize(Policy = "Finance.Invoice.Issue")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> IssueInvoice([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new IssueInvoiceCommand(id);
        var result = await _issueInvoiceHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Invoice issued successfully."));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "Finance.Invoice.Cancel")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelInvoice(
        [FromRoute] Guid id,
        [FromBody] CancelInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CancelInvoiceCommand(id, request.Reason);
        var result = await _cancelInvoiceHandler.HandleAsync(command, cancellationToken);

        return Ok(ResponseFactory.Success(result, "Invoice cancelled successfully."));
    }

    [HttpPost("{id:guid}/payments")]
    [Authorize(Policy = "Finance.Payment.Create")]
    [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> RecordPayment(
        [FromRoute] Guid id,
        [FromBody] RecordPaymentRequest request,
        [FromServices] IValidator<RecordPaymentCommand> validator,
        CancellationToken cancellationToken)
    {
        var command = new RecordPaymentCommand(
            id,
            request.Amount,
            request.Currency,
            request.PaidAt,
            request.Method,
            request.Reference,
            request.Notes);

        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var result = await _recordPaymentHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetInvoicePayments), new { id = id }, ResponseFactory.Success(result, "Payment recorded successfully."));
    }

    [HttpGet("{id:guid}/payments")]
    [Authorize(Policy = "Finance.Payment.Read")]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoicePayments([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var query = new GetInvoicePaymentsQuery(id);
        var result = await _getInvoicePaymentsHandler.HandleAsync(query, cancellationToken);

        return Ok(ResponseFactory.Success(result));
    }
}
