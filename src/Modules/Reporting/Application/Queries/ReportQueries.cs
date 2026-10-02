using BuildingBlocks.Application.CQRS;
using Modules.Reporting.Application.DTOs;

namespace Modules.Reporting.Application.Queries;

public record GetExecutiveSummaryQuery(
    DateTime? From = null,
    DateTime? To = null
) : IQuery<ExecutiveSummaryDto>;

public record GetLeadReportQuery(
    DateTime? From = null,
    DateTime? To = null,
    string? Status = null,
    string? Source = null,
    Guid? AssignedTo = null
) : IQuery<LeadReportDto>;

public record GetSalesPipelineReportQuery(
    DateTime? From = null,
    DateTime? To = null,
    string? Stage = null,
    Guid? AssignedTo = null
) : IQuery<SalesPipelineReportDto>;

public record GetCustomerReportQuery(
    DateTime? From = null,
    DateTime? To = null,
    string? Status = null,
    Guid? AccountManagerId = null
) : IQuery<CustomerReportDto>;

public record GetProjectReportQuery(
    DateTime? From = null,
    DateTime? To = null,
    string? Status = null,
    Guid? CustomerId = null
) : IQuery<ProjectReportDto>;

public record GetFinanceReportQuery(
    DateTime? From = null,
    DateTime? To = null,
    string? Currency = null,
    Guid? CustomerId = null
) : IQuery<FinanceReportDto>;

public record GetSupportReportQuery(
    DateTime? From = null,
    DateTime? To = null,
    string? Priority = null,
    Guid? CustomerId = null,
    Guid? AssignedTo = null
) : IQuery<SupportReportDto>;

public record GetActivityReportQuery(
    DateTime? From = null,
    DateTime? To = null,
    string? Type = null,
    Guid? UserId = null
) : IQuery<ActivityReportDto>;
