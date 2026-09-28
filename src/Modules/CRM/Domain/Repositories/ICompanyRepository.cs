using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Domain.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Company>> GetListAsync(int page, int pageSize, string? search, string? type, string? industry, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? search, string? type, string? industry, CancellationToken cancellationToken = default);
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
    void Update(Company company);
}
