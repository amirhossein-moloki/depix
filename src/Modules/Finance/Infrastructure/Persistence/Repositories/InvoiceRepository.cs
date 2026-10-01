using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.Finance.Domain.Entities;
using Modules.Finance.Domain.Repositories;

namespace Modules.Finance.Infrastructure.Persistence.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _dbContext;

    public InvoiceRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Invoice>()
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Invoice>()
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task<(List<Invoice> Items, int TotalCount)> GetFilteredAsync(InvoiceFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<Invoice>()
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .AsQueryable();

        if (filterParams.CustomerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == filterParams.CustomerId.Value);
        }

        if (filterParams.ProjectId.HasValue)
        {
            query = query.Where(i => i.ProjectId == filterParams.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Status))
        {
            var statusUpper = filterParams.Status.ToUpperInvariant();
            query = query.Where(i => i.Status == statusUpper);
        }

        if (filterParams.FromIssueDate.HasValue)
        {
            query = query.Where(i => i.IssueDate >= filterParams.FromIssueDate.Value);
        }

        if (filterParams.ToIssueDate.HasValue)
        {
            query = query.Where(i => i.IssueDate <= filterParams.ToIssueDate.Value);
        }

        if (filterParams.FromDueDate.HasValue)
        {
            query = query.Where(i => i.DueDate >= filterParams.FromDueDate.Value);
        }

        if (filterParams.ToDueDate.HasValue)
        {
            query = query.Where(i => i.DueDate <= filterParams.ToDueDate.Value);
        }

        if (filterParams.IsOverdue.HasValue && filterParams.IsOverdue.Value)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(i => (i.Status == "ISSUED" || i.Status == "PARTIALLY_PAID" || i.Status == "OVERDUE") && i.DueDate < today && i.OutstandingBalance.Amount > 0);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var term = filterParams.SearchTerm.Trim().ToLower();
            query = query.Where(i => i.InvoiceNumber.ToLower().Contains(term) || i.Notes.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = filterParams.Page <= 0 ? 1 : filterParams.Page;
        var pageSize = filterParams.PageSize <= 0 ? 10 : filterParams.PageSize;

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Invoice>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Invoice>()
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Invoice>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Invoice>()
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Where(i => i.ProjectId == projectId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsInvoiceNumberAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Invoice>()
            .AnyAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Invoice>().AddAsync(invoice, cancellationToken);
    }

    public void Update(Invoice invoice)
    {
        _dbContext.Set<Invoice>().Update(invoice);
    }
}
