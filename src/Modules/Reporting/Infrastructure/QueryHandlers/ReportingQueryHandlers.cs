using BuildingBlocks.Application.CQRS;
using Microsoft.EntityFrameworkCore;
using CRMCompany = Modules.CRM.Domain.Entities.Company;
using CRMContact = Modules.CRM.Domain.Entities.Contact;
using CRMLead = Modules.CRM.Domain.Entities.Lead;
using CRMActivity = Modules.CRM.Domain.Entities.Activity;
using CRMSalesNote = Modules.CRM.Domain.Entities.SalesNote;
using CustomerEntity = Modules.Customer.Domain.Entities.Customer;
using ProjectEntity = Modules.Project.Domain.Entities.Project;
using FinanceInvoice = Modules.Finance.Domain.Entities.Invoice;
using FinancePayment = Modules.Finance.Domain.Entities.Payment;
using SalesOpportunity = Modules.Sales.Domain.Entities.Opportunity;
using SupportTicket = Modules.Support.Domain.Entities.Ticket;
using Modules.Reporting.Application.DTOs;
using Modules.Reporting.Application.Queries;

namespace Modules.Reporting.Infrastructure.QueryHandlers;

public class ReportingQueryHandlers :
    IQueryHandler<GetExecutiveSummaryQuery, ExecutiveSummaryDto>,
    IQueryHandler<GetLeadReportQuery, LeadReportDto>,
    IQueryHandler<GetSalesPipelineReportQuery, SalesPipelineReportDto>,
    IQueryHandler<GetCustomerReportQuery, CustomerReportDto>,
    IQueryHandler<GetProjectReportQuery, ProjectReportDto>,
    IQueryHandler<GetFinanceReportQuery, FinanceReportDto>,
    IQueryHandler<GetSupportReportQuery, SupportReportDto>,
    IQueryHandler<GetActivityReportQuery, ActivityReportDto>
{
    private readonly DbContext _dbContext;

    public ReportingQueryHandlers(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExecutiveSummaryDto> HandleAsync(GetExecutiveSummaryQuery query, CancellationToken cancellationToken = default)
    {
        var fromDate = query.From ?? DateTime.MinValue;
        var toDate = query.To ?? DateTime.MaxValue;

        // CRM Metrics
        var companiesQuery = _dbContext.Set<CRMCompany>().AsNoTracking().Where(c => !c.IsDeleted);
        var contactsQuery = _dbContext.Set<CRMContact>().AsNoTracking().Where(c => !c.IsDeleted);
        var leadsQuery = _dbContext.Set<CRMLead>().AsNoTracking().Where(l => !l.IsDeleted);

        if (query.From.HasValue) leadsQuery = leadsQuery.Where(l => l.CreatedAt >= query.From.Value);
        if (query.To.HasValue) leadsQuery = leadsQuery.Where(l => l.CreatedAt <= query.To.Value);

        var totalCompanies = await companiesQuery.CountAsync(cancellationToken);
        var totalContacts = await contactsQuery.CountAsync(cancellationToken);
        var totalLeads = await leadsQuery.CountAsync(cancellationToken);
        var activeLeads = await leadsQuery.CountAsync(l => l.Status.ToLower() != "converted" && l.Status.ToLower() != "disqualified", cancellationToken);
        var qualifiedLeads = await leadsQuery.CountAsync(l => l.Status.ToLower() == "qualified", cancellationToken);
        var convertedLeads = await leadsQuery.CountAsync(l => l.Status.ToLower() == "converted", cancellationToken);
        var disqualifiedLeads = await leadsQuery.CountAsync(l => l.Status.ToLower() == "disqualified", cancellationToken);

        var crmSummary = new ExecutiveSummaryCrmDto(
            totalCompanies,
            totalContacts,
            totalLeads,
            activeLeads,
            qualifiedLeads,
            convertedLeads,
            disqualifiedLeads
        );

        // Sales Metrics
        var oppsQuery = _dbContext.Set<SalesOpportunity>().AsNoTracking().Where(o => !o.IsDeleted);
        if (query.From.HasValue) oppsQuery = oppsQuery.Where(o => o.CreatedAt >= query.From.Value);
        if (query.To.HasValue) oppsQuery = oppsQuery.Where(o => o.CreatedAt <= query.To.Value);

        var totalOpps = await oppsQuery.CountAsync(cancellationToken);
        var openOpps = await oppsQuery.CountAsync(o => o.Status.ToLower() == "open", cancellationToken);
        var wonCount = await oppsQuery.CountAsync(o => o.Status.ToLower() == "won", cancellationToken);
        var lostCount = await oppsQuery.CountAsync(o => o.Status.ToLower() == "lost", cancellationToken);

        var pipelineEstimatedValList = await oppsQuery.Where(o => o.Status.ToLower() == "open").Select(o => o.Value != null ? o.Value.Amount : 0m).ToListAsync(cancellationToken);
        var pipelineEstimatedVal = pipelineEstimatedValList.Sum();

        var wonValList = await oppsQuery.Where(o => o.Status.ToLower() == "won").Select(o => o.Value != null ? o.Value.Amount : 0m).ToListAsync(cancellationToken);
        var wonVal = wonValList.Sum();

        var lostValList = await oppsQuery.Where(o => o.Status.ToLower() == "lost").Select(o => o.Value != null ? o.Value.Amount : 0m).ToListAsync(cancellationToken);
        var lostVal = lostValList.Sum();

        var oppsByStageGroup = await oppsQuery
            .GroupBy(o => o.Stage)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var salesSummary = new ExecutiveSummarySalesDto(
            openOpps,
            totalOpps,
            pipelineEstimatedVal,
            wonVal,
            lostVal,
            wonCount,
            lostCount,
            oppsByStageGroup
        );

        // Customer Metrics
        var customersQuery = _dbContext.Set<CustomerEntity>().AsNoTracking().Where(c => !c.IsDeleted);
        var totalCustomers = await customersQuery.CountAsync(cancellationToken);
        var activeCustomers = await customersQuery.CountAsync(c => c.Status.ToLower() == "active", cancellationToken);
        var inactiveCustomers = await customersQuery.CountAsync(c => c.Status.ToLower() == "inactive", cancellationToken);

        var newlyAcquiredQuery = customersQuery;
        if (query.From.HasValue) newlyAcquiredQuery = newlyAcquiredQuery.Where(c => c.CreatedAt >= query.From.Value);
        if (query.To.HasValue) newlyAcquiredQuery = newlyAcquiredQuery.Where(c => c.CreatedAt <= query.To.Value);
        var newlyAcquired = await newlyAcquiredQuery.CountAsync(cancellationToken);

        var customerSummary = new ExecutiveSummaryCustomerDto(
            totalCustomers,
            activeCustomers,
            inactiveCustomers,
            newlyAcquired
        );

        // Project Metrics
        var projectsQuery = _dbContext.Set<ProjectEntity>().AsNoTracking().Where(p => !p.IsDeleted);
        if (query.From.HasValue) projectsQuery = projectsQuery.Where(p => p.CreatedAt >= query.From.Value);
        if (query.To.HasValue) projectsQuery = projectsQuery.Where(p => p.CreatedAt <= query.To.Value);

        var totalProjects = await projectsQuery.CountAsync(cancellationToken);
        var activeProjects = await projectsQuery.CountAsync(p => p.Status.ToLower() == "inprogress" || p.Status.ToLower() == "active" || p.Status.ToLower() == "in_progress", cancellationToken);
        var completedProjects = await projectsQuery.CountAsync(p => p.Status.ToLower() == "completed", cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdueProjects = await projectsQuery.CountAsync(p => p.Status.ToLower() != "completed" && p.PlannedDeliveryDate.HasValue && p.PlannedDeliveryDate.Value < today, cancellationToken);

        var projectsByStatusGroup = await projectsQuery
            .GroupBy(p => p.Status)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var projectSummary = new ExecutiveSummaryProjectDto(
            totalProjects,
            activeProjects,
            completedProjects,
            overdueProjects,
            projectsByStatusGroup
        );

        // Finance Metrics
        var invoicesQuery = _dbContext.Set<FinanceInvoice>().AsNoTracking().Where(i => !i.IsDeleted);
        if (query.From.HasValue) invoicesQuery = invoicesQuery.Where(i => i.IssueDate >= DateOnly.FromDateTime(query.From.Value));
        if (query.To.HasValue) invoicesQuery = invoicesQuery.Where(i => i.IssueDate <= DateOnly.FromDateTime(query.To.Value));

        var totalInvoices = await invoicesQuery.CountAsync(cancellationToken);
        var invList = await invoicesQuery.Select(i => new {
            Total = i.Total != null ? i.Total.Amount : 0m,
            PaidAmount = i.PaidAmount != null ? i.PaidAmount.Amount : 0m,
            OutstandingBalance = i.OutstandingBalance != null ? i.OutstandingBalance.Amount : 0m,
            DueDate = i.DueDate
        }).ToListAsync(cancellationToken);

        var totalInvoiced = invList.Sum(i => i.Total);
        var totalPaid = invList.Sum(i => i.PaidAmount);
        var totalOutstanding = invList.Sum(i => i.OutstandingBalance);
        var totalOverdue = invList.Where(i => i.DueDate < today && i.OutstandingBalance > 0).Sum(i => i.OutstandingBalance);

        var financeSummary = new ExecutiveSummaryFinanceDto(
            totalInvoices,
            totalInvoiced,
            totalPaid,
            totalOutstanding,
            totalOverdue,
            "USD"
        );

        // Support Metrics
        var ticketsQuery = _dbContext.Set<SupportTicket>().AsNoTracking().Where(t => !t.IsDeleted);
        if (query.From.HasValue) ticketsQuery = ticketsQuery.Where(t => t.CreatedAt >= query.From.Value);
        if (query.To.HasValue) ticketsQuery = ticketsQuery.Where(t => t.CreatedAt <= query.To.Value);

        var totalTickets = await ticketsQuery.CountAsync(cancellationToken);
        var openTickets = await ticketsQuery.CountAsync(t => t.Status.ToLower() == "open" || t.Status.ToLower() == "in_progress", cancellationToken);
        var resolvedTickets = await ticketsQuery.CountAsync(t => t.Status.ToLower() == "resolved" || t.Status.ToLower() == "closed", cancellationToken);

        var resolvedTicketTimes = await ticketsQuery
            .Where(t => (t.Status.ToLower() == "resolved" || t.Status.ToLower() == "closed") && t.ResolvedAt.HasValue)
            .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            .ToListAsync(cancellationToken);

        var avgResolutionHours = resolvedTicketTimes.Count > 0 ? Math.Round(resolvedTicketTimes.Average(), 2) : 0.0;

        var ticketsByStatusGroup = await ticketsQuery
            .GroupBy(t => t.Status)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var ticketsByPriorityGroup = await ticketsQuery
            .GroupBy(t => t.Priority)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var supportSummary = new ExecutiveSummarySupportDto(
            totalTickets,
            openTickets,
            resolvedTickets,
            avgResolutionHours,
            ticketsByStatusGroup,
            ticketsByPriorityGroup
        );

        return new ExecutiveSummaryDto(
            fromDate,
            toDate,
            crmSummary,
            salesSummary,
            customerSummary,
            projectSummary,
            financeSummary,
            supportSummary
        );
    }

    public async Task<LeadReportDto> HandleAsync(GetLeadReportQuery query, CancellationToken cancellationToken = default)
    {
        var leadsQuery = _dbContext.Set<CRMLead>().AsNoTracking().Where(l => !l.IsDeleted);

        if (query.From.HasValue) leadsQuery = leadsQuery.Where(l => l.CreatedAt >= query.From.Value);
        if (query.To.HasValue) leadsQuery = leadsQuery.Where(l => l.CreatedAt <= query.To.Value);
        if (!string.IsNullOrWhiteSpace(query.Status)) leadsQuery = leadsQuery.Where(l => l.Status.ToLower() == query.Status.Trim().ToLower());
        if (!string.IsNullOrWhiteSpace(query.Source)) leadsQuery = leadsQuery.Where(l => l.Source.ToLower() == query.Source.Trim().ToLower());
        if (query.AssignedTo.HasValue && query.AssignedTo != Guid.Empty) leadsQuery = leadsQuery.Where(l => l.AssignedTo == query.AssignedTo.Value);

        var totalLeads = await leadsQuery.CountAsync(cancellationToken);
        var activeLeads = await leadsQuery.CountAsync(l => l.Status.ToLower() != "converted" && l.Status.ToLower() != "disqualified", cancellationToken);
        var qualifiedLeads = await leadsQuery.CountAsync(l => l.Status.ToLower() == "qualified", cancellationToken);
        var convertedLeads = await leadsQuery.CountAsync(l => l.Status.ToLower() == "converted", cancellationToken);
        var disqualifiedLeads = await leadsQuery.CountAsync(l => l.Status.ToLower() == "disqualified", cancellationToken);

        var leadsByStatus = await leadsQuery
            .GroupBy(l => l.Status)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var leadsBySource = await leadsQuery
            .GroupBy(l => l.Source)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var leadsByAssigned = await leadsQuery
            .Where(l => l.AssignedTo.HasValue)
            .GroupBy(l => l.AssignedTo!.Value.ToString())
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var createdOverTimeList = await leadsQuery
            .Select(l => new { l.CreatedAt })
            .ToListAsync(cancellationToken);

        var createdOverTime = createdOverTimeList
            .GroupBy(l => l.CreatedAt.ToString("yyyy-MM"))
            .Select(g => new TimeSeriesPointDto(g.Key, g.Count()))
            .OrderBy(p => p.Period)
            .ToList();

        return new LeadReportDto(
            totalLeads,
            activeLeads,
            qualifiedLeads,
            convertedLeads,
            disqualifiedLeads,
            leadsByStatus,
            leadsBySource,
            leadsByAssigned,
            createdOverTime
        );
    }

    public async Task<SalesPipelineReportDto> HandleAsync(GetSalesPipelineReportQuery query, CancellationToken cancellationToken = default)
    {
        var oppsQuery = _dbContext.Set<SalesOpportunity>().AsNoTracking().Where(o => !o.IsDeleted);

        if (query.From.HasValue) oppsQuery = oppsQuery.Where(o => o.CreatedAt >= query.From.Value);
        if (query.To.HasValue) oppsQuery = oppsQuery.Where(o => o.CreatedAt <= query.To.Value);
        if (!string.IsNullOrWhiteSpace(query.Stage)) oppsQuery = oppsQuery.Where(o => o.Stage.ToLower() == query.Stage.Trim().ToLower());
        if (query.AssignedTo.HasValue && query.AssignedTo != Guid.Empty) oppsQuery = oppsQuery.Where(o => o.AssignedTo == query.AssignedTo.Value);

        var totalOpps = await oppsQuery.CountAsync(cancellationToken);
        var openOpps = await oppsQuery.CountAsync(o => o.Status.ToLower() == "open", cancellationToken);
        var wonOpps = await oppsQuery.CountAsync(o => o.Status.ToLower() == "won", cancellationToken);
        var lostOpps = await oppsQuery.CountAsync(o => o.Status.ToLower() == "lost", cancellationToken);

        var oppsList = await oppsQuery.Select(o => new {
            Amount = o.Value != null ? o.Value.Amount : 0m,
            o.Status,
            o.Stage,
            o.Probability,
            o.AssignedTo,
            o.CreatedAt
        }).ToListAsync(cancellationToken);

        var totalVal = oppsList.Sum(o => o.Amount);
        var wonVal = oppsList.Where(o => o.Status.ToLower() == "won").Sum(o => o.Amount);
        var lostVal = oppsList.Where(o => o.Status.ToLower() == "lost").Sum(o => o.Amount);
        var avgVal = totalOpps > 0 ? Math.Round(totalVal / totalOpps, 2) : 0m;
        var totalWeightedVal = oppsList.Sum(o => o.Amount * (o.Probability / 100m));

        var oppsByStage = oppsList
            .GroupBy(o => o.Stage)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToList();

        var valByStage = oppsList
            .GroupBy(o => o.Stage)
            .Select(g => new KeyValueAmountDto(g.Key, g.Sum(o => o.Amount), "USD"))
            .ToList();

        var oppsByAssigned = oppsList
            .Where(o => o.AssignedTo.HasValue)
            .GroupBy(o => o.AssignedTo!.Value.ToString())
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToList();

        var createdOverTime = oppsList
            .GroupBy(o => o.CreatedAt.ToString("yyyy-MM"))
            .Select(g => new TimeSeriesPointDto(g.Key, g.Count(), g.Sum(x => x.Amount), "USD"))
            .OrderBy(p => p.Period)
            .ToList();

        return new SalesPipelineReportDto(
            totalOpps,
            openOpps,
            wonOpps,
            lostOpps,
            totalVal,
            totalWeightedVal,
            wonVal,
            lostVal,
            avgVal,
            oppsByStage,
            valByStage,
            oppsByAssigned,
            createdOverTime
        );
    }

    public async Task<CustomerReportDto> HandleAsync(GetCustomerReportQuery query, CancellationToken cancellationToken = default)
    {
        var custQuery = _dbContext.Set<CustomerEntity>().AsNoTracking().Where(c => !c.IsDeleted);

        if (query.From.HasValue) custQuery = custQuery.Where(c => c.CreatedAt >= query.From.Value);
        if (query.To.HasValue) custQuery = custQuery.Where(c => c.CreatedAt <= query.To.Value);
        if (!string.IsNullOrWhiteSpace(query.Status)) custQuery = custQuery.Where(c => c.Status.ToLower() == query.Status.Trim().ToLower());
        if (query.AccountManagerId.HasValue && query.AccountManagerId != Guid.Empty) custQuery = custQuery.Where(c => c.AssignedTo == query.AccountManagerId.Value);

        var totalCust = await custQuery.CountAsync(cancellationToken);
        var activeCust = await custQuery.CountAsync(c => c.Status.ToLower() == "active", cancellationToken);
        var inactiveCust = await custQuery.CountAsync(c => c.Status.ToLower() == "inactive", cancellationToken);

        var createdOverTimeList = await custQuery
            .Select(c => new { c.CreatedAt })
            .ToListAsync(cancellationToken);

        var acqPeriod = createdOverTimeList
            .GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .OrderBy(p => p.Key)
            .ToList();

        var byAccountManager = await custQuery
            .Where(c => c.AssignedTo.HasValue)
            .GroupBy(c => c.AssignedTo!.Value.ToString())
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        return new CustomerReportDto(
            totalCust,
            activeCust,
            inactiveCust,
            acqPeriod,
            byAccountManager
        );
    }

    public async Task<ProjectReportDto> HandleAsync(GetProjectReportQuery query, CancellationToken cancellationToken = default)
    {
        var projQuery = _dbContext.Set<ProjectEntity>().AsNoTracking().Where(p => !p.IsDeleted);

        if (query.From.HasValue) projQuery = projQuery.Where(p => p.CreatedAt >= query.From.Value);
        if (query.To.HasValue) projQuery = projQuery.Where(p => p.CreatedAt <= query.To.Value);
        if (!string.IsNullOrWhiteSpace(query.Status)) projQuery = projQuery.Where(p => p.Status.ToLower() == query.Status.Trim().ToLower());
        if (query.CustomerId.HasValue && query.CustomerId != Guid.Empty) projQuery = projQuery.Where(p => p.CustomerId == query.CustomerId.Value);

        var totalProjs = await projQuery.CountAsync(cancellationToken);
        var activeProjs = await projQuery.CountAsync(p => p.Status.ToLower() == "inprogress" || p.Status.ToLower() == "active" || p.Status.ToLower() == "in_progress", cancellationToken);
        var completedProjs = await projQuery.CountAsync(p => p.Status.ToLower() == "completed", cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdueProjs = await projQuery.CountAsync(p => p.Status.ToLower() != "completed" && p.PlannedDeliveryDate.HasValue && p.PlannedDeliveryDate.Value < today, cancellationToken);

        var byStatus = await projQuery
            .GroupBy(p => p.Status)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var byCustomer = await projQuery
            .GroupBy(p => p.CustomerId.ToString())
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var startedList = await projQuery
            .Where(p => p.StartDate.HasValue)
            .Select(p => new { StartDate = p.StartDate!.Value })
            .ToListAsync(cancellationToken);

        var startedOverTime = startedList
            .GroupBy(p => $"{p.StartDate.Year:D4}-{p.StartDate.Month:D2}")
            .Select(g => new TimeSeriesPointDto(g.Key, g.Count()))
            .OrderBy(p => p.Period)
            .ToList();

        return new ProjectReportDto(
            totalProjs,
            activeProjs,
            completedProjs,
            overdueProjs,
            byStatus,
            byCustomer,
            startedOverTime
        );
    }

    public async Task<FinanceReportDto> HandleAsync(GetFinanceReportQuery query, CancellationToken cancellationToken = default)
    {
        var invQuery = _dbContext.Set<FinanceInvoice>().AsNoTracking().Where(i => !i.IsDeleted);

        if (query.From.HasValue) invQuery = invQuery.Where(i => i.IssueDate >= DateOnly.FromDateTime(query.From.Value));
        if (query.To.HasValue) invQuery = invQuery.Where(i => i.IssueDate <= DateOnly.FromDateTime(query.To.Value));
        if (!string.IsNullOrWhiteSpace(query.Currency)) invQuery = invQuery.Where(i => i.Currency.ToLower() == query.Currency.Trim().ToLower());
        if (query.CustomerId.HasValue && query.CustomerId != Guid.Empty) invQuery = invQuery.Where(i => i.CustomerId == query.CustomerId.Value);

        var totalInvoices = await invQuery.CountAsync(cancellationToken);
        var invList = await invQuery.Select(i => new {
            Subtotal = i.Subtotal != null ? i.Subtotal.Amount : 0m,
            Tax = i.Tax != null ? i.Tax.Amount : 0m,
            Discount = i.Discount != null ? i.Discount.Amount : 0m,
            Total = i.Total != null ? i.Total.Amount : 0m,
            PaidAmount = i.PaidAmount != null ? i.PaidAmount.Amount : 0m,
            OutstandingBalance = i.OutstandingBalance != null ? i.OutstandingBalance.Amount : 0m,
            Currency = i.Currency,
            Status = i.Status,
            DueDate = i.DueDate
        }).ToListAsync(cancellationToken);

        var totalSubtotal = invList.Sum(i => i.Subtotal);
        var totalTax = invList.Sum(i => i.Tax);
        var totalDiscount = invList.Sum(i => i.Discount);
        var totalInvoiced = invList.Sum(i => i.Total);
        var totalPaid = invList.Sum(i => i.PaidAmount);
        var totalOutstanding = invList.Sum(i => i.OutstandingBalance);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var totalOverdue = invList.Where(i => i.DueDate < today && i.OutstandingBalance > 0).Sum(i => i.OutstandingBalance);

        var byStatus = invList
            .GroupBy(i => i.Status)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToList();

        var byCurrency = invList
            .GroupBy(i => i.Currency)
            .Select(g => new KeyValueAmountDto(g.Key, g.Sum(i => i.Total), g.Key))
            .ToList();

        var paymentsQuery = _dbContext.Set<FinancePayment>().AsNoTracking();
        if (query.From.HasValue) paymentsQuery = paymentsQuery.Where(p => p.PaidAt >= query.From.Value);
        if (query.To.HasValue) paymentsQuery = paymentsQuery.Where(p => p.PaidAt <= query.To.Value);

        var paymentList = await paymentsQuery
            .Select(p => new { p.PaidAt, Amount = p.Amount != null ? p.Amount.Amount : 0m, Currency = p.Amount != null ? p.Amount.Currency : "USD" })
            .ToListAsync(cancellationToken);

        var paymentsOverTime = paymentList
            .GroupBy(p => p.PaidAt.ToString("yyyy-MM"))
            .Select(g => new TimeSeriesPointDto(g.Key, g.Count(), g.Sum(x => x.Amount), g.FirstOrDefault()?.Currency ?? "USD"))
            .OrderBy(p => p.Period)
            .ToList();

        return new FinanceReportDto(
            totalInvoices,
            totalSubtotal,
            totalTax,
            totalDiscount,
            totalInvoiced,
            totalPaid,
            totalOutstanding,
            totalOverdue,
            byStatus,
            byCurrency,
            paymentsOverTime
        );
    }

    public async Task<SupportReportDto> HandleAsync(GetSupportReportQuery query, CancellationToken cancellationToken = default)
    {
        var ticketQuery = _dbContext.Set<SupportTicket>().AsNoTracking().Where(t => !t.IsDeleted);

        if (query.From.HasValue) ticketQuery = ticketQuery.Where(t => t.CreatedAt >= query.From.Value);
        if (query.To.HasValue) ticketQuery = ticketQuery.Where(t => t.CreatedAt <= query.To.Value);
        if (!string.IsNullOrWhiteSpace(query.Priority)) ticketQuery = ticketQuery.Where(t => t.Priority.ToLower() == query.Priority.Trim().ToLower());
        if (query.CustomerId.HasValue && query.CustomerId != Guid.Empty) ticketQuery = ticketQuery.Where(t => t.CustomerId == query.CustomerId.Value);
        if (query.AssignedTo.HasValue && query.AssignedTo != Guid.Empty) ticketQuery = ticketQuery.Where(t => t.AssignedToUserId == query.AssignedTo.Value);

        var totalTickets = await ticketQuery.CountAsync(cancellationToken);
        var openTickets = await ticketQuery.CountAsync(t => t.Status.ToLower() == "open", cancellationToken);
        var inProgressTickets = await ticketQuery.CountAsync(t => t.Status.ToLower() == "in_progress", cancellationToken);
        var resolvedTickets = await ticketQuery.CountAsync(t => t.Status.ToLower() == "resolved", cancellationToken);
        var closedTickets = await ticketQuery.CountAsync(t => t.Status.ToLower() == "closed", cancellationToken);

        var resolvedTicketTimes = await ticketQuery
            .Where(t => (t.Status.ToLower() == "resolved" || t.Status.ToLower() == "closed") && t.ResolvedAt.HasValue)
            .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
            .ToListAsync(cancellationToken);

        var avgResolutionHours = resolvedTicketTimes.Count > 0 ? Math.Round(resolvedTicketTimes.Average(), 2) : 0.0;

        var byStatus = await ticketQuery
            .GroupBy(t => t.Status)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var byPriority = await ticketQuery
            .GroupBy(t => t.Priority)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var byCustomer = await ticketQuery
            .GroupBy(t => t.CustomerId.ToString())
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var byAssigned = await ticketQuery
            .Where(t => t.AssignedToUserId.HasValue)
            .GroupBy(t => t.AssignedToUserId!.Value.ToString())
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var createdOverTimeList = await ticketQuery
            .Select(t => new { t.CreatedAt })
            .ToListAsync(cancellationToken);

        var createdOverTime = createdOverTimeList
            .GroupBy(t => t.CreatedAt.ToString("yyyy-MM"))
            .Select(g => new TimeSeriesPointDto(g.Key, g.Count()))
            .OrderBy(p => p.Period)
            .ToList();

        return new SupportReportDto(
            totalTickets,
            openTickets,
            inProgressTickets,
            resolvedTickets,
            closedTickets,
            avgResolutionHours,
            byStatus,
            byPriority,
            byCustomer,
            byAssigned,
            createdOverTime
        );
    }

    public async Task<ActivityReportDto> HandleAsync(GetActivityReportQuery query, CancellationToken cancellationToken = default)
    {
        var actQuery = _dbContext.Set<CRMActivity>().AsNoTracking().Where(a => !a.IsDeleted);

        if (query.From.HasValue) actQuery = actQuery.Where(a => a.OccurredAt >= query.From.Value);
        if (query.To.HasValue) actQuery = actQuery.Where(a => a.OccurredAt <= query.To.Value);
        if (!string.IsNullOrWhiteSpace(query.Type)) actQuery = actQuery.Where(a => a.Type.ToLower() == query.Type.Trim().ToLower());
        if (query.UserId.HasValue && query.UserId != Guid.Empty) actQuery = actQuery.Where(a => a.UserId == query.UserId.Value);

        var totalActivities = await actQuery.CountAsync(cancellationToken);
        var completedActivities = await actQuery.CountAsync(a => a.OccurredAt <= DateTime.UtcNow, cancellationToken);
        var overdueActivities = await actQuery.CountAsync(a => a.FollowUpAt.HasValue && a.FollowUpAt.Value < DateTime.UtcNow, cancellationToken);

        var noteQuery = _dbContext.Set<CRMSalesNote>().AsNoTracking().Where(n => !n.IsDeleted);
        if (query.From.HasValue) noteQuery = noteQuery.Where(n => n.CreatedAt >= query.From.Value);
        if (query.To.HasValue) noteQuery = noteQuery.Where(n => n.CreatedAt <= query.To.Value);
        if (query.UserId.HasValue && query.UserId != Guid.Empty) noteQuery = noteQuery.Where(n => n.CreatedBy == query.UserId.Value);

        var totalSalesNotes = await noteQuery.CountAsync(cancellationToken);

        var qualityScores = await actQuery.Select(a => a.QualityScore).ToListAsync(cancellationToken);
        var avgQualityScore = qualityScores.Count > 0 ? Math.Round(qualityScores.Average(), 2) : 0.0;

        var byType = await actQuery
            .GroupBy(a => a.Type)
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var byUser = await actQuery
            .Where(a => a.UserId.HasValue)
            .GroupBy(a => a.UserId!.Value.ToString())
            .Select(g => new KeyValueCountDto(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        var actList = await actQuery
            .Select(a => new { a.OccurredAt })
            .ToListAsync(cancellationToken);

        var actOverTime = actList
            .GroupBy(a => a.OccurredAt.ToString("yyyy-MM"))
            .Select(g => new TimeSeriesPointDto(g.Key, g.Count()))
            .OrderBy(p => p.Period)
            .ToList();

        return new ActivityReportDto(
            totalActivities,
            completedActivities,
            overdueActivities,
            totalSalesNotes,
            avgQualityScore,
            byType,
            byUser,
            actOverTime
        );
    }
}
