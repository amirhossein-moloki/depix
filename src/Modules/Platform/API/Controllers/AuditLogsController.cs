using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Platform.Application.DTOs;

namespace Modules.Platform.API.Controllers;

[ApiController]
[Route("api/platform/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IQueryHandler<GetAuditLogsQuery, PagedAuditLogResult> _getAuditLogsHandler;
    private readonly IQueryHandler<GetAuditLogByIdQuery, AuditLogDto> _getAuditLogByIdHandler;
    private readonly IValidator<GetAuditLogsQuery> _queryValidator;

    public AuditLogsController(
        IQueryHandler<GetAuditLogsQuery, PagedAuditLogResult> getAuditLogsHandler,
        IQueryHandler<GetAuditLogByIdQuery, AuditLogDto> getAuditLogByIdHandler,
        IValidator<GetAuditLogsQuery> queryValidator)
    {
        _getAuditLogsHandler = getAuditLogsHandler;
        _getAuditLogByIdHandler = getAuditLogByIdHandler;
        _queryValidator = queryValidator;
    }

    [HttpGet]
    [Authorize(Policy = "Platform.Audit.Read")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery(entityType, entityId, userId, action, page, pageSize);

        var validationResult = await _queryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Get audit logs validation failed.", errors);
        }

        var result = await _getAuditLogsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Audit logs retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "Platform.Audit.Read")]
    public async Task<IActionResult> GetAuditLogById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAuditLogByIdQuery(id);
        var auditLog = await _getAuditLogByIdHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(auditLog, "Audit log retrieved successfully."));
    }
}
