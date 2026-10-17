using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.SalesVisits.DTOs;
using Modules.CRM.Application.Features.SalesVisits.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.SalesVisits.Queries;

public record GetCompanyVisitHistoryQuery(Guid CompanyId) : IQuery<List<SalesVisitDto>>;

public class GetCompanyVisitHistoryQueryHandler : IQueryHandler<GetCompanyVisitHistoryQuery, List<SalesVisitDto>>
{
    private readonly ISalesVisitRepository _salesVisitRepository;
    private readonly ICompanyRepository _companyRepository;

    public GetCompanyVisitHistoryQueryHandler(
        ISalesVisitRepository salesVisitRepository,
        ICompanyRepository companyRepository)
    {
        _salesVisitRepository = salesVisitRepository;
        _companyRepository = companyRepository;
    }

    public async Task<List<SalesVisitDto>> HandleAsync(GetCompanyVisitHistoryQuery query, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(query.CompanyId, cancellationToken);
        if (company == null)
        {
            throw new EntityNotFoundException(nameof(Company), query.CompanyId);
        }

        var visits = await _salesVisitRepository.GetByCompanyIdAsync(query.CompanyId, cancellationToken);
        return visits.Select(v => v.ToDto()).ToList();
    }
}
