using BuildingBlocks.Application.CQRS;

namespace Modules.Platform.Application.DTOs;

public record AppSettingDto(
    Guid Id,
    string Key,
    string Value,
    string? Description,
    string Category,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateSettingRequest(
    string Key,
    string Value,
    string? Description,
    string Category = "General",
    bool IsActive = true);

public record UpdateSettingRequest(
    string Value,
    string? Description,
    string Category = "General",
    bool IsActive = true);

public record CreateSettingCommand(
    string Key,
    string Value,
    string? Description,
    string Category = "General",
    bool IsActive = true) : ICommand<AppSettingDto>;

public record UpdateSettingCommand(
    string Key,
    string Value,
    string? Description,
    string Category = "General",
    bool IsActive = true) : ICommand<AppSettingDto>;

public record DeleteSettingCommand(
    string Key,
    Guid? DeletedBy = null) : ICommand;

public record GetSettingsQuery(
    string? Category = null,
    bool? IsActive = null) : IQuery<List<AppSettingDto>>;

public record GetSettingByKeyQuery(
    string Key) : IQuery<AppSettingDto>;
