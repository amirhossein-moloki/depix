using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.DTOs;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Services;

public interface ISalesVisitService
{
    Task<SalesVisitDto> CreateVisitAsync(Guid salespersonId, CreateSalesVisitRequest request, CancellationToken cancellationToken = default);
    Task<SalesVisitDto> GetVisitByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<SalesVisitDto>> GetVisitsAsync(
        Guid? companyId = null,
        Guid? salespersonId = null,
        string? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    );
}

public class SalesVisitService : ISalesVisitService
{
    private readonly ISalesVisitRepository _salesVisitRepository;
    private readonly ICompanyRepository _companyRepository;

    public SalesVisitService(ISalesVisitRepository salesVisitRepository, ICompanyRepository companyRepository)
    {
        _salesVisitRepository = salesVisitRepository;
        _companyRepository = companyRepository;
    }

    public async Task<SalesVisitDto> CreateVisitAsync(Guid salespersonId, CreateSalesVisitRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ValidationException("Company ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Report))
        {
            throw new ValidationException("Report text is required.");
        }

        var company = await _companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company == null)
        {
            throw new EntityNotFoundException("Company", request.CompanyId);
        }

        var visitDate = request.VisitDate ?? DateTime.UtcNow;

        var visit = SalesVisit.Create(
            request.CompanyId,
            salespersonId,
            visitDate,
            request.Report,
            request.Result,
            request.CustomerId,
            request.FollowUpAt,
            request.Latitude,
            request.Longitude,
            request.LocationSource
        );

        await _salesVisitRepository.AddAsync(visit, cancellationToken);

        return MapToDto(visit);
    }

    public async Task<SalesVisitDto> GetVisitByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var visit = await _salesVisitRepository.GetByIdAsync(id, cancellationToken);
        if (visit == null)
        {
            throw new EntityNotFoundException("SalesVisit", id);
        }

        return MapToDto(visit);
    }

    public async Task<PagedResult<SalesVisitDto>> GetVisitsAsync(
        Guid? companyId = null,
        Guid? salespersonId = null,
        string? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : (pageSize > 100 ? 100 : pageSize);

        var visits = await _salesVisitRepository.GetVisitsAsync(companyId, salespersonId, result, fromDate, toDate, page, pageSize, cancellationToken);
        var totalCount = await _salesVisitRepository.GetVisitsCountAsync(companyId, salespersonId, result, fromDate, toDate, cancellationToken);

        var dtos = visits.Select(MapToDto).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<SalesVisitDto>(dtos, totalCount, page, pageSize, totalPages);
    }

    private static SalesVisitDto MapToDto(SalesVisit visit)
    {
        return new SalesVisitDto(
            visit.Id,
            visit.CompanyId,
            visit.CustomerId,
            visit.SalespersonId,
            visit.VisitDate,
            visit.Report,
            visit.Result,
            visit.FollowUpAt,
            visit.Latitude,
            visit.Longitude,
            visit.LocationSource,
            visit.CreatedAt
        );
    }
}
