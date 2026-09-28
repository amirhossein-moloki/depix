using Modules.Finance.Domain.Entities;

namespace Modules.Finance.Domain.Repositories;

public interface IFinancialTransactionRepository
{
    Task<FinancialTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(FinancialTransaction transaction, CancellationToken cancellationToken = default);
    void Update(FinancialTransaction transaction);
}
