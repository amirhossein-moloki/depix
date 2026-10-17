using Modules.CRM.Application.Features.SalesVisits.DTOs;
using Modules.CRM.Domain.Entities;

namespace Modules.CRM.Application.Features.SalesVisits.Mappings;

public static class SalesVisitMappingExtensions
{
    public static SalesVisitDto ToDto(this SalesVisit visit)
    {
        return new SalesVisitDto(
            visit.Id,
            visit.CompanyId,
            visit.SalespersonId,
            visit.VisitedAt,
            visit.ReportText,
            visit.Result,
            visit.FollowUpAt,
            visit.Latitude,
            visit.Longitude,
            visit.LocationSource,
            visit.CreatedAt,
            visit.UpdatedAt
        );
    }

    public static SalesVisitListDto ToListDto(this SalesVisit visit)
    {
        return new SalesVisitListDto(
            visit.Id,
            visit.CompanyId,
            visit.SalespersonId,
            visit.VisitedAt,
            visit.Result,
            visit.Latitude,
            visit.Longitude,
            visit.CreatedAt
        );
    }
}
