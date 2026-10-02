using BuildingBlocks.Application.CQRS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Reporting.Application.DTOs;
using Modules.Reporting.Application.Queries;
using Modules.Reporting.Infrastructure.QueryHandlers;

namespace Modules.Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ReportingQueryHandlers>();

        services.AddScoped<IQueryHandler<GetExecutiveSummaryQuery, ExecutiveSummaryDto>>(sp => sp.GetRequiredService<ReportingQueryHandlers>());
        services.AddScoped<IQueryHandler<GetLeadReportQuery, LeadReportDto>>(sp => sp.GetRequiredService<ReportingQueryHandlers>());
        services.AddScoped<IQueryHandler<GetSalesPipelineReportQuery, SalesPipelineReportDto>>(sp => sp.GetRequiredService<ReportingQueryHandlers>());
        services.AddScoped<IQueryHandler<GetCustomerReportQuery, CustomerReportDto>>(sp => sp.GetRequiredService<ReportingQueryHandlers>());
        services.AddScoped<IQueryHandler<GetProjectReportQuery, ProjectReportDto>>(sp => sp.GetRequiredService<ReportingQueryHandlers>());
        services.AddScoped<IQueryHandler<GetFinanceReportQuery, FinanceReportDto>>(sp => sp.GetRequiredService<ReportingQueryHandlers>());
        services.AddScoped<IQueryHandler<GetSupportReportQuery, SupportReportDto>>(sp => sp.GetRequiredService<ReportingQueryHandlers>());
        services.AddScoped<IQueryHandler<GetActivityReportQuery, ActivityReportDto>>(sp => sp.GetRequiredService<ReportingQueryHandlers>());

        return services;
    }
}
