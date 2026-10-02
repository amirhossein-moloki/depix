namespace Modules.Reporting.Application.DTOs;

public record KeyValueCountDto(string Key, int Count);

public record KeyValueAmountDto(string Key, decimal Amount, string Currency = "USD");

public record TimeSeriesPointDto(string Period, int Count, decimal TotalAmount = 0m, string Currency = "USD");

public record ExecutiveSummaryCrmDto(
    int TotalCompanies,
    int TotalContacts,
    int TotalLeads,
    int ActiveLeads,
    int QualifiedLeads,
    int ConvertedLeads,
    int DisqualifiedLeads
);

public record ExecutiveSummarySalesDto(
    int OpenOpportunities,
    int TotalOpportunities,
    decimal PipelineEstimatedValue,
    decimal WonOpportunityValue,
    decimal LostOpportunityValue,
    int WonCount,
    int LostCount,
    List<KeyValueCountDto> OpportunitiesByStage
);

public record ExecutiveSummaryCustomerDto(
    int TotalCustomers,
    int ActiveCustomers,
    int InactiveCustomers,
    int NewlyAcquiredInPeriod
);

public record ExecutiveSummaryProjectDto(
    int TotalProjects,
    int ActiveProjects,
    int CompletedProjects,
    int OverdueProjects,
    List<KeyValueCountDto> ProjectsByStatus
);

public record ExecutiveSummaryFinanceDto(
    int TotalInvoices,
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal TotalOutstanding,
    decimal TotalOverdue,
    string PrimaryCurrency
);

public record ExecutiveSummarySupportDto(
    int TotalTickets,
    int OpenTickets,
    int ResolvedTickets,
    double AverageResolutionTimeHours,
    List<KeyValueCountDto> TicketsByStatus,
    List<KeyValueCountDto> TicketsByPriority
);

public record ExecutiveSummaryDto(
    DateTime FilterFrom,
    DateTime FilterTo,
    ExecutiveSummaryCrmDto CRM,
    ExecutiveSummarySalesDto Sales,
    ExecutiveSummaryCustomerDto Customer,
    ExecutiveSummaryProjectDto Project,
    ExecutiveSummaryFinanceDto Finance,
    ExecutiveSummarySupportDto Support
);

public record LeadReportDto(
    int TotalLeads,
    int ActiveLeads,
    int QualifiedLeads,
    int ConvertedLeads,
    int DisqualifiedLeads,
    List<KeyValueCountDto> LeadsByStatus,
    List<KeyValueCountDto> LeadsBySource,
    List<KeyValueCountDto> LeadsByAssignedUser,
    List<TimeSeriesPointDto> LeadsCreatedOverTime
);

public record SalesPipelineReportDto(
    int TotalOpportunities,
    int OpenOpportunities,
    int WonOpportunities,
    int LostOpportunities,
    decimal TotalPipelineValue,
    decimal TotalWeightedValue,
    decimal WonValue,
    decimal LostValue,
    decimal AverageOpportunityValue,
    List<KeyValueCountDto> OpportunitiesByStage,
    List<KeyValueAmountDto> ValueByStage,
    List<KeyValueCountDto> OpportunitiesByAssignedUser,
    List<TimeSeriesPointDto> OpportunitiesCreatedOverTime
);

public record CustomerReportDto(
    int TotalCustomers,
    int ActiveCustomers,
    int InactiveCustomers,
    List<KeyValueCountDto> CustomersByAcquisitionPeriod,
    List<KeyValueCountDto> CustomersByAccountManager
);

public record ProjectReportDto(
    int TotalProjects,
    int ActiveProjects,
    int CompletedProjects,
    int OverdueProjects,
    List<KeyValueCountDto> ProjectsByStatus,
    List<KeyValueCountDto> ProjectsByCustomer,
    List<TimeSeriesPointDto> ProjectsStartedOverTime
);

public record FinanceReportDto(
    int TotalInvoices,
    decimal TotalSubtotal,
    decimal TotalTax,
    decimal TotalDiscount,
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal TotalOutstanding,
    decimal TotalOverdue,
    List<KeyValueCountDto> InvoicesByStatus,
    List<KeyValueAmountDto> InvoicedByCurrency,
    List<TimeSeriesPointDto> PaymentsOverTime
);

public record SupportReportDto(
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int ResolvedTickets,
    int ClosedTickets,
    double AverageResolutionTimeHours,
    List<KeyValueCountDto> TicketsByStatus,
    List<KeyValueCountDto> TicketsByPriority,
    List<KeyValueCountDto> TicketsByCustomer,
    List<KeyValueCountDto> TicketsByAssignedUser,
    List<TimeSeriesPointDto> TicketsCreatedOverTime
);

public record ActivityReportDto(
    int TotalActivities,
    int CompletedActivities,
    int OverdueActivities,
    int TotalSalesNotes,
    double AverageQualityScore,
    List<KeyValueCountDto> ActivitiesByType,
    List<KeyValueCountDto> ActivitiesByUser,
    List<TimeSeriesPointDto> ActivitiesOverTime
);
