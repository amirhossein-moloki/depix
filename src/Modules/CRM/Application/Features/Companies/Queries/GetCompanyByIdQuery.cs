using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Companies.Queries;

public record GetCompanyByIdQuery(Guid Id) : IQuery<CompanyDto>;

public class GetCompanyByIdQueryHandler : IQueryHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;

    public GetCompanyByIdQueryHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<CompanyDto> HandleAsync(GetCompanyByIdQuery query, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(query.Id, cancellationToken);
        if (company == null)
        {
            throw new EntityNotFoundException(nameof(Company), query.Id);
        }

        return company.ToDto();
    }
}
