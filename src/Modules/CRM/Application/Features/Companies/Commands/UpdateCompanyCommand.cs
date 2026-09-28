using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Domain.ValueObjects;

namespace Modules.CRM.Application.Features.Companies.Commands;

public record UpdateCompanyCommand(
    Guid Id,
    string Name,
    string Industry,
    string Website,
    string Phone,
    string Email,
    string AddressText,
    string Type
) : ICommand<CompanyDto>;

public class UpdateCompanyCommandHandler : ICommandHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCompanyCommandHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompanyDto> HandleAsync(UpdateCompanyCommand command, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(command.Id, cancellationToken);
        if (company == null)
        {
            throw new EntityNotFoundException(nameof(Company), command.Id);
        }

        var address = new Address(command.AddressText);
        company.UpdateInfo(
            command.Name,
            command.Industry,
            command.Website,
            command.Phone,
            command.Email,
            address,
            command.Type
        );

        _companyRepository.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return company.ToDto();
    }
}
