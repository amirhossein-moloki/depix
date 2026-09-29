using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.Contacts.Commands;
using Modules.CRM.Application.Features.Contacts.DTOs;
using Modules.CRM.Application.Features.Contacts.Queries;

namespace Modules.CRM.API.Controllers;

[ApiController]
[Route("api/crm/contacts")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly ICommandHandler<CreateContactCommand, ContactDto> _createContactHandler;
    private readonly ICommandHandler<UpdateContactCommand, ContactDto> _updateContactHandler;
    private readonly ICommandHandler<ArchiveContactCommand> _archiveContactHandler;
    private readonly IQueryHandler<GetContactByIdQuery, ContactDto> _getContactByIdHandler;
    private readonly IQueryHandler<GetContactsQuery, PagedResult<ContactListDto>> _getContactsHandler;
    private readonly IQueryHandler<SearchContactsQuery, PagedResult<ContactListDto>> _searchContactsHandler;
    private readonly IValidator<CreateContactCommand> _createValidator;
    private readonly IValidator<UpdateContactCommand> _updateValidator;

    public ContactsController(
        ICommandHandler<CreateContactCommand, ContactDto> createContactHandler,
        ICommandHandler<UpdateContactCommand, ContactDto> updateContactHandler,
        ICommandHandler<ArchiveContactCommand> archiveContactHandler,
        IQueryHandler<GetContactByIdQuery, ContactDto> getContactByIdHandler,
        IQueryHandler<GetContactsQuery, PagedResult<ContactListDto>> getContactsHandler,
        IQueryHandler<SearchContactsQuery, PagedResult<ContactListDto>> searchContactsHandler,
        IValidator<CreateContactCommand> createValidator,
        IValidator<UpdateContactCommand> updateValidator)
    {
        _createContactHandler = createContactHandler;
        _updateContactHandler = updateContactHandler;
        _archiveContactHandler = archiveContactHandler;
        _getContactByIdHandler = getContactByIdHandler;
        _getContactsHandler = getContactsHandler;
        _searchContactsHandler = searchContactsHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    [Authorize(Policy = "CRM.Contact.Create")]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateContactCommand(
            request.CompanyId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Position,
            request.Description,
            request.IsDecisionMaker,
            request.InfluenceLevel
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Contact validation failed.", errors);
        }

        var contact = await _createContactHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, ResponseFactory.Success(contact, "Contact created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "CRM.Contact.Read")]
    public async Task<IActionResult> GetContacts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? companyId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetContactsQuery(page, pageSize, companyId, search);
        var result = await _getContactsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Contacts retrieved successfully."));
    }

    [HttpGet("search")]
    [Authorize(Policy = "CRM.Contact.Read")]
    public async Task<IActionResult> SearchContacts(
        [FromQuery] string? search = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchContactsQuery(search, companyId, page, pageSize);
        var result = await _searchContactsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Contacts search results retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CRM.Contact.Read")]
    public async Task<IActionResult> GetContactById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetContactByIdQuery(id);
        var contact = await _getContactByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(contact, "Contact details retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CRM.Contact.Update")]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateContactCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.Position,
            request.Description,
            request.IsDecisionMaker,
            request.InfluenceLevel
        );

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Contact update validation failed.", errors);
        }

        var contact = await _updateContactHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(contact, "Contact updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CRM.Contact.Delete")]
    public async Task<IActionResult> ArchiveContact(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? archivedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new ArchiveContactCommand(id, archivedBy);
        await _archiveContactHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Contact archived successfully.", string.Empty));
    }
}
