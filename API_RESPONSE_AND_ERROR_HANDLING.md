# Unified API Response & Error Handling Architecture

This document describes the unified API response wrapper, exception handling pipeline, error codes, and architectural guidelines implemented across the CRM/ERP Modular Monolith.

---

## 1. Unified API Response Format

Every API endpoint across all domain modules returns responses adhering to a single, consistent API contract using `ApiResponse<T>` or `ApiResponse`.

### Success Response Example
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "companyName": "Acme Corp"
  },
  "errors": [],
  "traceId": "0HN012345678"
}
```

### Error Response Example (Validation Error)
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "field": "email",
      "message": "Email is invalid"
    }
  ],
  "traceId": "0HN012345678"
}
```

### Response Model Properties
- `success` (`bool`): Indicates whether the requested operation succeeded.
- `message` (`string`): Human-readable summary of the result.
- `data` (`T?`): Response payload object for successful requests (null for error responses).
- `errors` (`List<ApiError>`): List of structured errors containing `code`, `field` (optional), and `message`.
- `traceId` (`string`): Correlation trace identifier generated from `HttpContext.TraceIdentifier`.

---

## 2. Infrastructure Architecture

Shared infrastructure components are located under `BuildingBlocks/Common/`:

```
BuildingBlocks/Common/
├── Responses/
│   ├── ApiError.cs
│   ├── ApiResponse.cs
│   ├── ErrorCode.cs
│   ├── ResponseFactory.cs
│   └── ResponseExtensions.cs
├── Exceptions/
│   ├── BusinessRuleException.cs
│   ├── EntityNotFoundException.cs
│   ├── ValidationException.cs
│   ├── UnauthorizedException.cs
│   └── ForbiddenException.cs
├── Handlers/
│   └── GlobalExceptionHandler.cs
└── Extensions/
    └── ExceptionHandlingExtensions.cs
```

---

## 3. Global Exception Handling Flow

Centralized exception handling is implemented using ASP.NET Core's `IExceptionHandler` feature (`GlobalExceptionHandler`).

### Exception to HTTP Status Code Mapping

| Exception Type | HTTP Status Code | Description |
| :--- | :--- | :--- |
| `ValidationException` | `400 Bad Request` | Form/input validation failure with field-level errors. |
| `BusinessRuleException` | `400 Bad Request` | Violation of a business rule or domain invariant. |
| `EntityNotFoundException` | `404 Not Found` | Requested domain entity or resource does not exist. |
| `UnauthorizedException` / `UnauthorizedAccessException` | `401 Unauthorized` | Request lacks required authentication credentials. |
| `ForbiddenException` | `403 Forbidden` | Authenticated user lacks permission for the resource. |
| `Exception` (Unhandled) | `500 Internal Server Error` | Unexpected failure. Internal details are hidden from the client. |

### Logging Strategy
- **Unhandled/Unexpected Exceptions (HTTP 500)**: Logged at `LogError` level with stack trace, request method, path, and `traceId`.
- **Domain & Application Exceptions (HTTP 400, 401, 403, 404)**: Logged at `LogWarning` level with `traceId`, request method, path, and error summary. Stack traces are suppressed to avoid log noise.

---

## 4. Error Code System

Error codes are centralized in `ErrorCode.cs` as strongly-typed constants:

```csharp
public static class ErrorCode
{
    // System Error Codes
    public const string SystemError = "SYSTEM_001";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string BusinessRuleViolation = "BUSINESS_RULE_VIOLATION";

    // Domain Specific Codes
    public const string AuthError = "AUTH_001";
    public const string CrmError = "CRM_001";
    public const string SalesError = "SALES_001";
    public const string ProjectError = "PROJECT_001";
    public const string FinanceError = "FINANCE_001";
    public const string CustomerError = "CUSTOMER_001";
    public const string SupportError = "SUPPORT_001";
    public const string PlatformError = "PLATFORM_001";
}
```

### Adding New Error Codes
When adding new domain error codes:
1. Define a constant in `ErrorCode.cs` (e.g. `public const string ProposalExpired = "SALES_002";`).
2. Pass the error code when throwing domain/application exceptions or returning `Result.Failure(new Error(ErrorCode.ProposalExpired, "..."))`.

---

## 5. How Modules Should Use This System

### In Controllers
Controllers should return responses using `ResponseFactory` or extension methods:

```csharp
[ApiController]
[Route("api/crm/leads")]
public class LeadsController : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<LeadDto>>> GetLeadById(Guid id)
    {
        var lead = await _mediator.Send(new GetLeadQuery(id));
        return Ok(ResponseFactory.Success(lead, "Lead retrieved successfully", HttpContext.TraceIdentifier));
    }
}
```

### Throwing Exceptions in Domain / Application Logic
Modules should throw typed exceptions rather than returning raw framework errors:

```csharp
// Business Rule Failure
throw new BusinessRuleException("Lead cannot be converted without an active contact.", ErrorCode.CrmError);

// Entity Not Found
throw new EntityNotFoundException("Lead", leadId);

// Validation Failure
throw new ValidationException("Invalid lead details", new[]
{
    new ValidationErrorDetail("email", "Email address is invalid")
});
```

### Integration with Result Pattern
When handlers use `Result<T>` from `BuildingBlocks.SharedKernel`, convert them directly into `ApiResponse<T>`:

```csharp
Result<CustomerDto> result = await _customerService.GetCustomerAsync(id);
return result.ToApiResponse(HttpContext.TraceIdentifier);
```
