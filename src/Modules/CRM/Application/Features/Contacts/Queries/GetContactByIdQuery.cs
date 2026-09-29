using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Contacts.DTOs;
using Modules.CRM.Application.Features.Contacts.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Contacts.Queries;

public record GetContactByIdQuery(Guid Id) : IQuery<ContactDto>;

public class GetContactByIdQueryHandler : IQueryHandler<GetContactByIdQuery, ContactDto>
{
    private readonly IContactRepository _contactRepository;

    public GetContactByIdQueryHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<ContactDto> HandleAsync(GetContactByIdQuery query, CancellationToken cancellationToken = default)
    {
        var contact = await _contactRepository.GetByIdAsync(query.Id, cancellationToken);
        if (contact == null)
        {
            throw new EntityNotFoundException(nameof(Contact), query.Id);
        }

        return contact.ToDto();
    }
}
