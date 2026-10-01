using System.Security.Claims;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Platform.Application.DTOs;

namespace Modules.Platform.API.Controllers;

[ApiController]
[Route("api/platform/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ICommandHandler<CreateSettingCommand, AppSettingDto> _createSettingHandler;
    private readonly ICommandHandler<UpdateSettingCommand, AppSettingDto> _updateSettingHandler;
    private readonly ICommandHandler<DeleteSettingCommand> _deleteSettingHandler;
    private readonly IQueryHandler<GetSettingsQuery, List<AppSettingDto>> _getSettingsHandler;
    private readonly IQueryHandler<GetSettingByKeyQuery, AppSettingDto> _getSettingByKeyHandler;
    private readonly IValidator<CreateSettingCommand> _createValidator;
    private readonly IValidator<UpdateSettingCommand> _updateValidator;

    public SettingsController(
        ICommandHandler<CreateSettingCommand, AppSettingDto> createSettingHandler,
        ICommandHandler<UpdateSettingCommand, AppSettingDto> updateSettingHandler,
        ICommandHandler<DeleteSettingCommand> deleteSettingHandler,
        IQueryHandler<GetSettingsQuery, List<AppSettingDto>> getSettingsHandler,
        IQueryHandler<GetSettingByKeyQuery, AppSettingDto> getSettingByKeyHandler,
        IValidator<CreateSettingCommand> createValidator,
        IValidator<UpdateSettingCommand> updateValidator)
    {
        _createSettingHandler = createSettingHandler;
        _updateSettingHandler = updateSettingHandler;
        _deleteSettingHandler = deleteSettingHandler;
        _getSettingsHandler = getSettingsHandler;
        _getSettingByKeyHandler = getSettingByKeyHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    [Authorize(Policy = "Platform.Setting.Write")]
    public async Task<IActionResult> CreateSetting([FromBody] CreateSettingRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateSettingCommand(
            request.Key,
            request.Value,
            request.Description,
            request.Category,
            request.IsActive);

        var validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Setting creation validation failed.", errors);
        }

        var setting = await _createSettingHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetSettingByKey), new { key = setting.Key }, ResponseFactory.Success(setting, "Setting created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "Platform.Setting.Read")]
    public async Task<IActionResult> GetSettings(
        [FromQuery] string? category = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSettingsQuery(category, isActive);
        var settings = await _getSettingsHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(settings, "Settings retrieved successfully."));
    }

    [HttpGet("{key}")]
    [Authorize(Policy = "Platform.Setting.Read")]
    public async Task<IActionResult> GetSettingByKey(string key, CancellationToken cancellationToken)
    {
        var query = new GetSettingByKeyQuery(key);
        var setting = await _getSettingByKeyHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(setting, "Setting retrieved successfully."));
    }

    [HttpPut("{key}")]
    [Authorize(Policy = "Platform.Setting.Write")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateSettingCommand(
            key,
            request.Value,
            request.Description,
            request.Category,
            request.IsActive);

        var validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => new ValidationErrorDetail(e.PropertyName, e.ErrorMessage)).ToList();
            throw new BuildingBlocks.Common.Exceptions.ValidationException("Setting update validation failed.", errors);
        }

        var setting = await _updateSettingHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success(setting, "Setting updated successfully."));
    }

    [HttpDelete("{key}")]
    [Authorize(Policy = "Platform.Setting.Delete")]
    public async Task<IActionResult> DeleteSetting(string key, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        Guid? deletedBy = Guid.TryParse(userIdClaim, out var userId) ? userId : null;

        var command = new DeleteSettingCommand(key, deletedBy);
        await _deleteSettingHandler.HandleAsync(command, cancellationToken);
        return Ok(ResponseFactory.Success("Setting deleted successfully.", string.Empty));
    }
}
