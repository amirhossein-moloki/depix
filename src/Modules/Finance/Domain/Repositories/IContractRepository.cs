using Modules.Finance.Domain.Entities;

namespace Modules.Finance.Domain.Repositories;

public interface IContractRepository
{
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);
    void Update(Contract contract);
}
