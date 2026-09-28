namespace BuildingBlocks.Common.Responses;

public static class ErrorCode
{
    // General / System Error Codes
    public const string SystemError = "SYSTEM_001";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";

    // Domain / Module Specific Error Codes
    public const string AuthError = "AUTH_001";
    public const string CrmError = "CRM_001";
    public const string SalesError = "SALES_001";
    public const string ProjectError = "PROJECT_001";
    public const string FinanceError = "FINANCE_001";
    public const string CustomerError = "CUSTOMER_001";
    public const string SupportError = "SUPPORT_001";
    public const string PlatformError = "PLATFORM_001";
}
