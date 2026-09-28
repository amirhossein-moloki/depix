using Microsoft.EntityFrameworkCore;
using Modules.Finance.Domain.Entities;
using Modules.Finance.Domain.Repositories;

namespace Modules.Finance.Infrastructure.Persistence.Repositories;

public class FinancialTransactionRepository : IFinancialTransactionRepository
{
    private readonly DbContext _dbContext;

    public FinancialTransactionRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FinancialTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<FinancialTransaction>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(FinancialTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<FinancialTransaction>().AddAsync(transaction, cancellationToken);
    }

    public void Update(FinancialTransaction transaction)
    {
        _dbContext.Set<FinancialTransaction>().Update(transaction);
    }
}
