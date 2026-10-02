using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modules.Reporting.Application.DTOs;
using Modules.Reporting.Application.Queries;

namespace Modules.Reporting.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IQueryHandler<GetExecutiveSummaryQuery, ExecutiveSummaryDto> _executiveSummaryHandler;
    private readonly IQueryHandler<GetLeadReportQuery, LeadReportDto> _leadReportHandler;
    private readonly IQueryHandler<GetSalesPipelineReportQuery, SalesPipelineReportDto> _salesPipelineReportHandler;
    private readonly IQueryHandler<GetCustomerReportQuery, CustomerReportDto> _customerReportHandler;
    private readonly IQueryHandler<GetProjectReportQuery, ProjectReportDto> _projectReportHandler;
    private readonly IQueryHandler<GetFinanceReportQuery, FinanceReportDto> _financeReportHandler;
    private readonly IQueryHandler<GetSupportReportQuery, SupportReportDto> _supportReportHandler;
    private readonly IQueryHandler<GetActivityReportQuery, ActivityReportDto> _activityReportHandler;

    public ReportsController(
        IQueryHandler<GetExecutiveSummaryQuery, ExecutiveSummaryDto> executiveSummaryHandler,
        IQueryHandler<GetLeadReportQuery, LeadReportDto> leadReportHandler,
        IQueryHandler<GetSalesPipelineReportQuery, SalesPipelineReportDto> salesPipelineReportHandler,
        IQueryHandler<GetCustomerReportQuery, CustomerReportDto> customerReportHandler,
        IQueryHandler<GetProjectReportQuery, ProjectReportDto> projectReportHandler,
        IQueryHandler<GetFinanceReportQuery, FinanceReportDto> financeReportHandler,
        IQueryHandler<GetSupportReportQuery, SupportReportDto> supportReportHandler,
        IQueryHandler<GetActivityReportQuery, ActivityReportDto> activityReportHandler)
    {
        _executiveSummaryHandler = executiveSummaryHandler;
        _leadReportHandler = leadReportHandler;
        _salesPipelineReportHandler = salesPipelineReportHandler;
        _customerReportHandler = customerReportHandler;
        _projectReportHandler = projectReportHandler;
        _financeReportHandler = financeReportHandler;
        _supportReportHandler = supportReportHandler;
        _activityReportHandler = activityReportHandler;
    }

    [HttpGet("summary")]
    [Authorize(Policy = "Reporting.Read")]
    public async Task<IActionResult> GetExecutiveSummary(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetExecutiveSummaryQuery(from, to);
        var result = await _executiveSummaryHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Executive summary report retrieved successfully."));
    }

    [HttpGet("crm/leads")]
    [Authorize(Policy = "Reporting.CRM.Read")]
    public async Task<IActionResult> GetLeadReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? status = null,
        [FromQuery] string? source = null,
        [FromQuery] Guid? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLeadReportQuery(from, to, status, source, assignedTo);
        var result = await _leadReportHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Lead report retrieved successfully."));
    }

    [HttpGet("crm/activities")]
    [Authorize(Policy = "Reporting.CRM.Read")]
    public async Task<IActionResult> GetActivityReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? type = null,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetActivityReportQuery(from, to, type, userId);
        var result = await _activityReportHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Activity report retrieved successfully."));
    }

    [HttpGet("sales/pipeline")]
    [Authorize(Policy = "Reporting.Sales.Read")]
    public async Task<IActionResult> GetSalesPipelineReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? stage = null,
        [FromQuery] Guid? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSalesPipelineReportQuery(from, to, stage, assignedTo);
        var result = await _salesPipelineReportHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Sales pipeline report retrieved successfully."));
    }

    [HttpGet("customers")]
    [Authorize(Policy = "Reporting.Customer.Read")]
    public async Task<IActionResult> GetCustomerReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? accountManagerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerReportQuery(from, to, status, accountManagerId);
        var result = await _customerReportHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Customer report retrieved successfully."));
    }

    [HttpGet("projects")]
    [Authorize(Policy = "Reporting.Project.Read")]
    public async Task<IActionResult> GetProjectReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProjectReportQuery(from, to, status, customerId);
        var result = await _projectReportHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Project report retrieved successfully."));
    }

    [HttpGet("finance")]
    [Authorize(Policy = "Reporting.Finance.Read")]
    public async Task<IActionResult> GetFinanceReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? currency = null,
        [FromQuery] Guid? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFinanceReportQuery(from, to, currency, customerId);
        var result = await _financeReportHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Finance report retrieved successfully."));
    }

    [HttpGet("support")]
    [Authorize(Policy = "Reporting.Support.Read")]
    public async Task<IActionResult> GetSupportReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? priority = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSupportReportQuery(from, to, priority, customerId, assignedTo);
        var result = await _supportReportHandler.HandleAsync(query, cancellationToken);
        return Ok(ResponseFactory.Success(result, "Support report retrieved successfully."));
    }
}
