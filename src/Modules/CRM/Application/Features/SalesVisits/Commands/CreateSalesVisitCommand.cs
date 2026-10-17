using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.SalesVisits.DTOs;
using Modules.CRM.Application.Features.SalesVisits.Mappings;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.SalesVisits.Commands;

public record CreateSalesVisitCommand(
    Guid CompanyId,
    Guid SalespersonId,
    DateTime VisitedAt,
    string ReportText,
    string Result,
    DateTime? FollowUpAt = null,
    double? Latitude = null,
    double? Longitude = null,
    string? LocationSource = null
) : ICommand<SalesVisitDto>;

public class CreateSalesVisitCommandHandler : ICommandHandler<CreateSalesVisitCommand, SalesVisitDto>
{
    private readonly ISalesVisitRepository _salesVisitRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSalesVisitCommandHandler(
        ISalesVisitRepository salesVisitRepository,
        ICompanyRepository companyRepository,
        IUnitOfWork unitOfWork)
    {
        _salesVisitRepository = salesVisitRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesVisitDto> HandleAsync(CreateSalesVisitCommand command, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(command.CompanyId, cancellationToken);
        if (company == null)
        {
            throw new EntityNotFoundException(nameof(Company), command.CompanyId);
        }

        var visit = SalesVisit.Create(
            command.CompanyId,
            command.SalespersonId,
            command.VisitedAt,
            command.ReportText,
            command.Result,
            command.FollowUpAt,
            command.Latitude,
            command.Longitude,
            command.LocationSource
        );

        await _salesVisitRepository.AddAsync(visit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return visit.ToDto();
    }
}
