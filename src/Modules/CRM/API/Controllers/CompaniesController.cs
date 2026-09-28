using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.CRM.Application.Features.Companies.Commands;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.Companies.Queries;

namespace Modules.CRM.API.Controllers;

[ApiController]
[Route("api/crm/companies")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICommandHandler<CreateCompanyCommand, CompanyDto> _createCompanyHandler;
    private readonly ICommandHandler<UpdateCompanyCommand, CompanyDto> _updateCompanyHandler;
    private readonly ICommandHandler<ArchiveCompanyCommand> _archiveCompanyHandler;
    private readonly IQueryHandler<GetCompanyByIdQuery, CompanyDto> _getCompanyByIdHandler;
    private readonly IQueryHandler<GetCompaniesQuery, PagedResult<CompanyListDto>> _getCompaniesHandler;
    private readonly IValidator<CreateCompanyCommand> _createValidator;
    private readonly IValidator<UpdateCompanyCommand> _updateValidator;

    public CompaniesController(
        ICommandHandler<CreateCompanyCommand, CompanyDto> createCompanyHandler,
        ICommandHandler<UpdateCompanyCommand, CompanyDto> updateCompanyHandler,
        ICommandHandler<ArchiveCompanyCommand> archiveCompanyHandler,
        IQueryHandler<GetCompanyByIdQuery, CompanyDto> getCompanyByIdHandler,
        IQueryHandler<GetCompaniesQuery, PagedResult<CompanyListDto>> getCompaniesHandler,
        IValidator<CreateCompanyCommand> createValidator,
        IValidator<UpdateCompanyCommand> updateValidator)
    {
        _createCompanyHandler = createCompanyHandler;
        _updateCompanyHandler = updateCompanyHandler;
        _archiveCompanyHandler = archiveCompanyHandler;
        _getCompanyByIdHandler = getCompanyByIdHandler;
        _getCompaniesHandler = getCompaniesHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    [Authorize(Policy = "CRM.Company.Create")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateCompanyCommand(
            request.Name,
            request.Industry,
            request.Website,
            request.Phone,
            request.Email,
            request.AddressText,
            request.Type
        );

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Company validation failed.", errors);
        }

        var company = await _createCompanyHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetCompanyById), new { id = company.Id }, ResponseFactory.Success(company, "Company created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "CRM.Company.Read")]
    public async Task<IActionResult> GetCompanies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] string? industry = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCompaniesQuery(page, pageSize, search, type, industry);
        var result = await _getCompaniesHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Companies retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CRM.Company.Read")]
    public async Task<IActionResult> GetCompanyById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCompanyByIdQuery(id);
        var company = await _getCompanyByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(company, "Company details retrieved successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CRM.Company.Update")]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCompanyCommand(
            id,
            request.Name,
            request.Industry,
            request.Website,
            request.Phone,
            request.Email,
            request.AddressText,
            request.Type
        );

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Company update validation failed.", errors);
        }

        var company = await _updateCompanyHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(company, "Company updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CRM.Company.Delete")]
    public async Task<IActionResult> ArchiveCompany(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? archivedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new ArchiveCompanyCommand(id, archivedBy);
        await _archiveCompanyHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Company archived successfully.", string.Empty));
    }
}
