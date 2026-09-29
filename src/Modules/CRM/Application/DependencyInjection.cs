using BuildingBlocks.Application.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.CRM.Application.Features.Activities.Commands;
using Modules.CRM.Application.Features.Activities.DTOs;
using Modules.CRM.Application.Features.Activities.Queries;
using Modules.CRM.Application.Features.Activities.Validators;
using Modules.CRM.Application.Features.Companies.Commands;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Application.Features.Companies.Queries;
using Modules.CRM.Application.Features.Companies.Validators;
using Modules.CRM.Application.Features.Contacts.Commands;
using Modules.CRM.Application.Features.Contacts.DTOs;
using Modules.CRM.Application.Features.Contacts.Queries;
using Modules.CRM.Application.Features.Contacts.Validators;
using Modules.CRM.Application.Features.Leads.Commands;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Application.Features.Leads.Queries;
using Modules.CRM.Application.Features.Leads.Validators;
using Modules.CRM.Application.Features.SalesNotes.Commands;
using Modules.CRM.Application.Features.SalesNotes.DTOs;
using Modules.CRM.Application.Features.SalesNotes.Queries;
using Modules.CRM.Application.Features.SalesNotes.Validators;

namespace Modules.CRM.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCRMApplication(this IServiceCollection services)
    {
        // Companies Handlers
        services.AddScoped<ICommandHandler<CreateCompanyCommand, CompanyDto>, CreateCompanyCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCompanyCommand, CompanyDto>, UpdateCompanyCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveCompanyCommand>, ArchiveCompanyCommandHandler>();
        services.AddScoped<IQueryHandler<GetCompanyByIdQuery, CompanyDto>, GetCompanyByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetCompaniesQuery, PagedResult<CompanyListDto>>, GetCompaniesQueryHandler>();
        services.AddScoped<IQueryHandler<SearchCompaniesQuery, PagedResult<CompanyListDto>>, SearchCompaniesQueryHandler>();

        // Contacts Handlers
        services.AddScoped<ICommandHandler<CreateContactCommand, ContactDto>, CreateContactCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateContactCommand, ContactDto>, UpdateContactCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveContactCommand>, ArchiveContactCommandHandler>();
        services.AddScoped<IQueryHandler<GetContactByIdQuery, ContactDto>, GetContactByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetContactsQuery, PagedResult<ContactListDto>>, GetContactsQueryHandler>();
        services.AddScoped<IQueryHandler<SearchContactsQuery, PagedResult<ContactListDto>>, SearchContactsQueryHandler>();

        // Leads Handlers
        services.AddScoped<ICommandHandler<CreateLeadCommand, LeadDto>, CreateLeadCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateLeadCommand, LeadDto>, UpdateLeadCommandHandler>();
        services.AddScoped<ICommandHandler<AssignLeadCommand, LeadDto>, AssignLeadCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeLeadStatusCommand, LeadDto>, ChangeLeadStatusCommandHandler>();
        services.AddScoped<ICommandHandler<QualifyLeadCommand, LeadDto>, QualifyLeadCommandHandler>();
        services.AddScoped<ICommandHandler<DisqualifyLeadCommand, LeadDto>, DisqualifyLeadCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveLeadCommand>, ArchiveLeadCommandHandler>();

        services.AddScoped<IQueryHandler<GetLeadByIdQuery, LeadDto>, GetLeadByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetLeadsQuery, PagedResult<LeadListItemDto>>, GetLeadsQueryHandler>();
        services.AddScoped<IQueryHandler<SearchLeadsQuery, PagedResult<LeadListItemDto>>, SearchLeadsQueryHandler>();
        services.AddScoped<IQueryHandler<GetLeadPipelineQuery, LeadPipelineDto>, GetLeadPipelineQueryHandler>();

        // Activities Handlers
        services.AddScoped<ICommandHandler<CreateActivityCommand, ActivityDto>, CreateActivityCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateActivityCommand, ActivityDto>, UpdateActivityCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveActivityCommand>, ArchiveActivityCommandHandler>();
        services.AddScoped<IQueryHandler<GetActivityByIdQuery, ActivityDto>, GetActivityByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetActivitiesQuery, PagedResult<ActivityListItemDto>>, GetActivitiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetLeadActivitiesQuery, LeadActivityHistoryDto>, GetLeadActivitiesQueryHandler>();

        // SalesNotes Handlers
        services.AddScoped<ICommandHandler<CreateSalesNoteCommand, SalesNoteDto>, CreateSalesNoteCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateSalesNoteCommand, SalesNoteDto>, UpdateSalesNoteCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveSalesNoteCommand>, ArchiveSalesNoteCommandHandler>();
        services.AddScoped<IQueryHandler<GetSalesNoteByIdQuery, SalesNoteDto>, GetSalesNoteByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetSalesNotesQuery, PagedResult<SalesNoteListItemDto>>, GetSalesNotesQueryHandler>();
        services.AddScoped<IQueryHandler<GetLeadSalesNotesQuery, LeadSalesContextDto>, GetLeadSalesNotesQueryHandler>();

        // Validators
        services.AddScoped<IValidator<CreateCompanyCommand>, CreateCompanyCommandValidator>();
        services.AddScoped<IValidator<UpdateCompanyCommand>, UpdateCompanyCommandValidator>();
        services.AddScoped<IValidator<CreateContactCommand>, CreateContactCommandValidator>();
        services.AddScoped<IValidator<UpdateContactCommand>, UpdateContactCommandValidator>();

        services.AddScoped<IValidator<CreateLeadCommand>, CreateLeadCommandValidator>();
        services.AddScoped<IValidator<UpdateLeadCommand>, UpdateLeadCommandValidator>();
        services.AddScoped<IValidator<AssignLeadCommand>, AssignLeadCommandValidator>();
        services.AddScoped<IValidator<ChangeLeadStatusCommand>, ChangeLeadStatusCommandValidator>();
        services.AddScoped<IValidator<QualifyLeadCommand>, QualifyLeadCommandValidator>();
        services.AddScoped<IValidator<DisqualifyLeadCommand>, DisqualifyLeadCommandValidator>();

        services.AddScoped<IValidator<CreateActivityCommand>, CreateActivityCommandValidator>();
        services.AddScoped<IValidator<UpdateActivityCommand>, UpdateActivityCommandValidator>();

        services.AddScoped<IValidator<CreateSalesNoteCommand>, CreateSalesNoteCommandValidator>();
        services.AddScoped<IValidator<UpdateSalesNoteCommand>, UpdateSalesNoteCommandValidator>();

        return services;
    }
}
