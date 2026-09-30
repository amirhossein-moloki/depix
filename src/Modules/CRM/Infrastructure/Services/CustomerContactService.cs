using BuildingBlocks.Application.Contracts;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Infrastructure.Services;

public class CustomerContactService : ICustomerContactService
{
    private readonly IContactRepository _contactRepository;

    public CustomerContactService(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<List<CustomerContactContractDto>> GetContactsByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var contacts = await _contactRepository.GetListAsync(
            page: 1,
            pageSize: 1000,
            companyId: companyId,
            search: null,
            cancellationToken: cancellationToken);

        return contacts.Select(c => new CustomerContactContractDto(
            c.Id,
            c.CompanyId,
            c.Name,
            c.Position,
            c.Phone,
            c.Email,
            c.IsDecisionMaker,
            c.InfluenceLevel
        )).ToList();
    }

    public async Task<CustomerContactContractDto?> GetContactByIdAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        var c = await _contactRepository.GetByIdAsync(contactId, cancellationToken);
        if (c == null) return null;

        return new CustomerContactContractDto(
            c.Id,
            c.CompanyId,
            c.Name,
            c.Position,
            c.Phone,
            c.Email,
            c.IsDecisionMaker,
            c.InfluenceLevel
        );
    }
}
