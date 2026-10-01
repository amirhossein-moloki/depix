using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Modules.Support.Application.Services;
using Modules.Support.Domain.Entities;

namespace Modules.Support.Infrastructure.Services;

public class TicketNumberGenerator : ITicketNumberGenerator
{
    private readonly ApplicationDbContext _dbContext;

    public TicketNumberGenerator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default)
    {
        var yearMonth = DateTime.UtcNow.ToString("yyyyMM");
        var prefix = $"TICK-{yearMonth}-";

        var latestNumber = await _dbContext.Set<Ticket>()
            .IgnoreQueryFilters()
            .Where(t => t.TicketNumber.StartsWith(prefix))
            .Select(t => t.TicketNumber)
            .OrderByDescending(t => t)
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
