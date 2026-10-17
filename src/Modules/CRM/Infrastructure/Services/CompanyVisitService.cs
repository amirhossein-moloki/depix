using Microsoft.EntityFrameworkCore;
using Modules.CRM.Application.Features.Companies;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Infrastructure.Services;

public class CompanyVisitService : ICompanyVisitService
{
    private readonly DbContext _dbContext;
    public CompanyVisitService(DbContext dbContext) => _dbContext = dbContext;

    public async Task<CompanyVisitDto> CreateAsync(Guid sellerUserId, CreateCompanyVisitRequest request, CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Set<Company>().FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Company was not found.");
        var visit = CompanyVisit.Create(company.Id, sellerUserId, request.VisitedAt, request.Notes,
            request.Outcome, request.FollowUpAt, request.Latitude, request.Longitude);
        await _dbContext.Set<CompanyVisit>().AddAsync(visit, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new CompanyVisitDto(visit.Id, company.Id, company.Name, visit.SellerUserId, visit.VisitedAt,
            visit.Notes, visit.Outcome, visit.FollowUpAt, visit.Latitude, visit.Longitude);
    }

    public async Task<IReadOnlyList<CompanyVisitDto>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
        => await (from visit in _dbContext.Set<CompanyVisit>().AsNoTracking()
                  join company in _dbContext.Set<Company>().AsNoTracking() on visit.CompanyId equals company.Id
                  where visit.CompanyId == companyId
                  orderby visit.VisitedAt descending
                  select new CompanyVisitDto(visit.Id, company.Id, company.Name, visit.SellerUserId,
                      visit.VisitedAt, visit.Notes, visit.Outcome, visit.FollowUpAt, visit.Latitude, visit.Longitude))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CompanyMapPointDto>> GetMapPointsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<Company>().AsNoTracking()
            .Where(c => c.Latitude.HasValue && c.Longitude.HasValue)
            .OrderBy(c => c.Name)
            .Select(c => new CompanyMapPointDto(c.Id, c.Name, c.Address.Text, c.Type, c.Latitude!.Value, c.Longitude!.Value))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CompanyVisitMapPointDto>> GetVisitMapPointsAsync(CancellationToken cancellationToken = default)
        => await (from visit in _dbContext.Set<CompanyVisit>().AsNoTracking()
                  join company in _dbContext.Set<Company>().AsNoTracking() on visit.CompanyId equals company.Id
                  where visit.Latitude.HasValue && visit.Longitude.HasValue
                  orderby visit.VisitedAt descending
                  select new CompanyVisitMapPointDto(visit.Id, company.Id, company.Name, visit.SellerUserId,
                      visit.VisitedAt, visit.Notes, visit.Latitude!.Value, visit.Longitude!.Value))
            .ToListAsync(cancellationToken);
}
