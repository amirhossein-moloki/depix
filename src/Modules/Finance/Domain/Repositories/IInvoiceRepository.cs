using Modules.Finance.Domain.Entities;

namespace Modules.Finance.Domain.Repositories;

public record InvoiceFilterParams(
    int Page = 1,
    int PageSize = 10,
    Guid? CustomerId = null,
    Guid? ProjectId = null,
    string? Status = null,
    DateOnly? FromIssueDate = null,
    DateOnly? ToIssueDate = null,
    DateOnly? FromDueDate = null,
    DateOnly? ToDueDate = null,
    bool? IsOverdue = null,
    string? SearchTerm = null);

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<(List<Invoice> Items, int TotalCount)> GetFilteredAsync(InvoiceFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> ExistsInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);
    void Update(Invoice invoice);
}
