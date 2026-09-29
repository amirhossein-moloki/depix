using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Domain.Repositories;

public interface IContactRepository
{
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Contact>> GetListAsync(int page, int pageSize, Guid? companyId, string? search, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid? companyId, string? search, CancellationToken cancellationToken = default);
    Task AddAsync(Contact contact, CancellationToken cancellationToken = default);
    void Update(Contact contact);
}
