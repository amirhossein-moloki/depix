using BuildingBlocks.Application.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Application.Features.Opportunities.Commands;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Queries;
using Modules.Sales.Application.Features.Opportunities.Validators;

namespace Modules.Sales.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesApplication(this IServiceCollection services)
    {
        // Command Handlers
        services.AddScoped<ICommandHandler<CreateOpportunityCommand, OpportunityDto>, CreateOpportunityCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateOpportunityCommand, OpportunityDto>, UpdateOpportunityCommandHandler>();
        services.AddScoped<ICommandHandler<AssignOpportunityCommand, OpportunityDto>, AssignOpportunityCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeOpportunityStageCommand, OpportunityDto>, ChangeOpportunityStageCommandHandler>();
        services.AddScoped<ICommandHandler<MarkOpportunityWonCommand, OpportunityDto>, MarkOpportunityWonCommandHandler>();
        services.AddScoped<ICommandHandler<MarkOpportunityLostCommand, OpportunityDto>, MarkOpportunityLostCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveOpportunityCommand>, ArchiveOpportunityCommandHandler>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetOpportunityByIdQuery, OpportunityDto>, GetOpportunityByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetOpportunitiesQuery, PagedResult<OpportunityListItemDto>>, GetOpportunitiesQueryHandler>();
        services.AddScoped<IQueryHandler<SearchOpportunitiesQuery, PagedResult<OpportunityListItemDto>>, SearchOpportunitiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetOpportunityPipelineQuery, OpportunityPipelineDto>, GetOpportunityPipelineQueryHandler>();

        // Validators
        services.AddScoped<IValidator<CreateOpportunityCommand>, CreateOpportunityCommandValidator>();
        services.AddScoped<IValidator<UpdateOpportunityCommand>, UpdateOpportunityCommandValidator>();
        services.AddScoped<IValidator<AssignOpportunityCommand>, AssignOpportunityCommandValidator>();
        services.AddScoped<IValidator<ChangeOpportunityStageCommand>, ChangeOpportunityStageCommandValidator>();
        services.AddScoped<IValidator<MarkOpportunityWonCommand>, MarkOpportunityWonCommandValidator>();
        services.AddScoped<IValidator<MarkOpportunityLostCommand>, MarkOpportunityLostCommandValidator>();

        return services;
    }
}
