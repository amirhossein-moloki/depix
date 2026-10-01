using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.Finance.Application.Services;
using Modules.Finance.Domain.Entities;

namespace Modules.Finance.Infrastructure.Services;

public class InvoiceNumberGenerator : IInvoiceNumberGenerator
{
    private readonly ApplicationDbContext _dbContext;

    public InvoiceNumberGenerator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var yearMonth = DateTime.UtcNow.ToString("yyyyMM");
        var prefix = $"INV-{yearMonth}-";

        var latestNumber = await _dbContext.Set<Invoice>()
            .IgnoreQueryFilters()
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .Select(i => i.InvoiceNumber)
            .OrderByDescending(i => i)
            .FirstOrDefaultAsync(cancellationToken);

        int nextSequence = 1;

        if (!string.IsNullOrEmpty(latestNumber) && latestNumber.Length >= prefix.Length + 4)
        {
            var seqString = latestNumber.Substring(prefix.Length);
            if (int.TryParse(seqString, out int currentSequence))
            {
                nextSequence = currentSequence + 1;
            }
        }

        return $"{prefix}{nextSequence:D4}";
    }
}
