using BuildingBlocks.Application.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.Platform.Application.Commands;
using Modules.Platform.Application.DTOs;
using Modules.Platform.Application.Queries;
using Modules.Platform.Application.Validators;

namespace Modules.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApplication(this IServiceCollection services)
    {
        // File Command & Query Handlers
        services.AddScoped<ICommandHandler<UploadFileCommand, FileAssetDto>, UploadFileCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteFileCommand>, DeleteFileCommandHandler>();
        services.AddScoped<IQueryHandler<GetFileMetadataQuery, FileAssetDto>, GetFileMetadataQueryHandler>();
        services.AddScoped<IQueryHandler<GetFilesByEntityQuery, List<FileAssetDto>>, GetFilesByEntityQueryHandler>();
        services.AddScoped<IQueryHandler<DownloadFileQuery, FileDownloadDto>, DownloadFileQueryHandler>();

        // Audit Log Query Handlers
        services.AddScoped<IQueryHandler<GetAuditLogsQuery, PagedAuditLogResult>, GetAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<GetAuditLogByIdQuery, AuditLogDto>, GetAuditLogByIdQueryHandler>();

        // Setting Command & Query Handlers
        services.AddScoped<ICommandHandler<CreateSettingCommand, AppSettingDto>, CreateSettingCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateSettingCommand, AppSettingDto>, UpdateSettingCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteSettingCommand>, DeleteSettingCommandHandler>();
        services.AddScoped<IQueryHandler<GetSettingsQuery, List<AppSettingDto>>, GetSettingsQueryHandler>();
        services.AddScoped<IQueryHandler<GetSettingByKeyQuery, AppSettingDto>, GetSettingByKeyQueryHandler>();

        // Task Command & Query Handlers
        services.AddScoped<ICommandHandler<CreateTaskCommand, WorkTaskDto>, CreateTaskCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTaskStatusCommand, WorkTaskDto>, UpdateTaskStatusCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTaskCommand>, DeleteTaskCommandHandler>();
        services.AddScoped<IQueryHandler<GetTaskByIdQuery, WorkTaskDto>, GetTaskByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetTasksQuery, List<WorkTaskDto>>, GetTasksQueryHandler>();

        // Validators
        services.AddScoped<IValidator<UploadFileCommand>, UploadFileCommandValidator>();
        services.AddScoped<IValidator<CreateSettingCommand>, CreateSettingCommandValidator>();
        services.AddScoped<IValidator<UpdateSettingCommand>, UpdateSettingCommandValidator>();
        services.AddScoped<IValidator<CreateTaskCommand>, CreateTaskCommandValidator>();
        services.AddScoped<IValidator<GetAuditLogsQuery>, GetAuditLogsQueryValidator>();

        return services;
    }
}
