using Microsoft.EntityFrameworkCore;
using Modules.Finance.Domain.Entities;
using Modules.Finance.Domain.Repositories;

namespace Modules.Finance.Infrastructure.Persistence.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly DbContext _dbContext;

    public ContractRepository(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Contract>()
            .Include(c => c.ContractPayments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<Contract>().AddAsync(contract, cancellationToken);
    }

    public void Update(Contract contract)
    {
        _dbContext.Set<Contract>().Update(contract);
    }
}
