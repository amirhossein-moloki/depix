using BuildingBlocks.Application.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Application.Features.Opportunities.Commands;
using Modules.Sales.Application.Features.Opportunities.DTOs;
using Modules.Sales.Application.Features.Opportunities.Queries;
using Modules.Sales.Application.Features.Opportunities.Validators;
using Modules.Sales.Application.Features.Proposals.Commands;
using Modules.Sales.Application.Features.Proposals.DTOs;
using Modules.Sales.Application.Features.Proposals.Queries;
using Modules.Sales.Application.Features.Proposals.Validators;

namespace Modules.Sales.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSalesApplication(this IServiceCollection services)
    {
        // Opportunity Command Handlers
        services.AddScoped<ICommandHandler<CreateOpportunityCommand, OpportunityDto>, CreateOpportunityCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateOpportunityCommand, OpportunityDto>, UpdateOpportunityCommandHandler>();
        services.AddScoped<ICommandHandler<AssignOpportunityCommand, OpportunityDto>, AssignOpportunityCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeOpportunityStageCommand, OpportunityDto>, ChangeOpportunityStageCommandHandler>();
        services.AddScoped<ICommandHandler<MarkOpportunityWonCommand, OpportunityDto>, MarkOpportunityWonCommandHandler>();
        services.AddScoped<ICommandHandler<MarkOpportunityLostCommand, OpportunityDto>, MarkOpportunityLostCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveOpportunityCommand>, ArchiveOpportunityCommandHandler>();

        // Opportunity Query Handlers
        services.AddScoped<IQueryHandler<GetOpportunityByIdQuery, OpportunityDto>, GetOpportunityByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetOpportunitiesQuery, PagedResult<OpportunityListItemDto>>, GetOpportunitiesQueryHandler>();
        services.AddScoped<IQueryHandler<SearchOpportunitiesQuery, PagedResult<OpportunityListItemDto>>, SearchOpportunitiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetOpportunityPipelineQuery, OpportunityPipelineDto>, GetOpportunityPipelineQueryHandler>();

        // Opportunity Validators
        services.AddScoped<IValidator<CreateOpportunityCommand>, CreateOpportunityCommandValidator>();
        services.AddScoped<IValidator<UpdateOpportunityCommand>, UpdateOpportunityCommandValidator>();
        services.AddScoped<IValidator<AssignOpportunityCommand>, AssignOpportunityCommandValidator>();
        services.AddScoped<IValidator<ChangeOpportunityStageCommand>, ChangeOpportunityStageCommandValidator>();
        services.AddScoped<IValidator<MarkOpportunityWonCommand>, MarkOpportunityWonCommandValidator>();
        services.AddScoped<IValidator<MarkOpportunityLostCommand>, MarkOpportunityLostCommandValidator>();

        // Proposal Command Handlers
        services.AddScoped<ICommandHandler<CreateProposalCommand, ProposalDto>, CreateProposalCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProposalCommand, ProposalDto>, UpdateProposalCommandHandler>();
        services.AddScoped<ICommandHandler<AddProposalItemCommand, ProposalDto>, AddProposalItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProposalItemCommand, ProposalDto>, UpdateProposalItemCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveProposalItemCommand, ProposalDto>, RemoveProposalItemCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeProposalStatusCommand, ProposalDto>, ChangeProposalStatusCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveProposalCommand>, ArchiveProposalCommandHandler>();

        // Proposal Query Handlers
        services.AddScoped<IQueryHandler<GetProposalByIdQuery, ProposalDto>, GetProposalByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetProposalsQuery, PagedResult<ProposalListItemDto>>, GetProposalsQueryHandler>();
        services.AddScoped<IQueryHandler<GetOpportunityProposalsQuery, List<ProposalListItemDto>>, GetOpportunityProposalsQueryHandler>();

        // Proposal Validators
        services.AddScoped<IValidator<CreateProposalCommand>, CreateProposalCommandValidator>();
        services.AddScoped<IValidator<UpdateProposalCommand>, UpdateProposalCommandValidator>();
        services.AddScoped<IValidator<AddProposalItemCommand>, AddProposalItemCommandValidator>();
        services.AddScoped<IValidator<UpdateProposalItemCommand>, UpdateProposalItemCommandValidator>();
        services.AddScoped<IValidator<ChangeProposalStatusCommand>, ChangeProposalStatusCommandValidator>();

        return services;
    }
}
