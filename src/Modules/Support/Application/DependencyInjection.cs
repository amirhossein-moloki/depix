using BuildingBlocks.Application.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.Support.Application.Commands;
using Modules.Support.Application.DTOs;
using Modules.Support.Application.Queries;
using Modules.Support.Application.Validators;

namespace Modules.Support.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSupportApplication(this IServiceCollection services)
    {
        // Command Handlers
        services.AddScoped<ICommandHandler<CreateTicketCommand, TicketDetailDto>, CreateTicketCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTicketCommand, TicketDetailDto>, UpdateTicketCommandHandler>();
        services.AddScoped<ICommandHandler<AssignTicketCommand, TicketDetailDto>, AssignTicketCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeTicketPriorityCommand, TicketDetailDto>, ChangeTicketPriorityCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeTicketStatusCommand, TicketDetailDto>, ChangeTicketStatusCommandHandler>();
        services.AddScoped<ICommandHandler<ResolveTicketCommand, TicketDetailDto>, ResolveTicketCommandHandler>();
        services.AddScoped<ICommandHandler<CloseTicketCommand, TicketDetailDto>, CloseTicketCommandHandler>();
        services.AddScoped<ICommandHandler<CancelTicketCommand, TicketDetailDto>, CancelTicketCommandHandler>();
        services.AddScoped<ICommandHandler<ReopenTicketCommand, TicketDetailDto>, ReopenTicketCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteTicketCommand, bool>, DeleteTicketCommandHandler>();
        services.AddScoped<ICommandHandler<AddTicketCommentCommand, TicketCommentDto>, AddTicketCommentCommandHandler>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetTicketByIdQuery, TicketDetailDto>, GetTicketByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetTicketsQuery, PagedResult<TicketListItemDto>>, GetTicketsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomerTicketsQuery, List<TicketListItemDto>>, GetCustomerTicketsQueryHandler>();
        services.AddScoped<IQueryHandler<GetProjectTicketsQuery, List<TicketListItemDto>>, GetProjectTicketsQueryHandler>();
        services.AddScoped<IQueryHandler<GetTicketCommentsQuery, List<TicketCommentDto>>, GetTicketCommentsQueryHandler>();

        // Validators
        services.AddScoped<IValidator<CreateTicketCommand>, CreateTicketCommandValidator>();
        services.AddScoped<IValidator<UpdateTicketCommand>, UpdateTicketCommandValidator>();
        services.AddScoped<IValidator<AssignTicketCommand>, AssignTicketCommandValidator>();
        services.AddScoped<IValidator<ChangeTicketPriorityCommand>, ChangeTicketPriorityCommandValidator>();
        services.AddScoped<IValidator<ChangeTicketStatusCommand>, ChangeTicketStatusCommandValidator>();
        services.AddScoped<IValidator<ResolveTicketCommand>, ResolveTicketCommandValidator>();
        services.AddScoped<IValidator<AddTicketCommentCommand>, AddTicketCommentCommandValidator>();

        return services;
    }
}
