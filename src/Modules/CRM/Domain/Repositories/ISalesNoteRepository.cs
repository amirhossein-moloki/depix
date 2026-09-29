using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Domain.Repositories;

public interface ISalesNoteRepository
{
    Task<SalesNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SalesNote>> GetListAsync(
        int page,
        int pageSize,
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? createdBy,
        string? search,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Guid? leadId,
        Guid? contactId,
        Guid? companyId,
        Guid? createdBy,
        string? search,
        CancellationToken cancellationToken = default);

    Task AddAsync(SalesNote salesNote, CancellationToken cancellationToken = default);
    void Update(SalesNote salesNote);
}
