using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using Modules.CRM.Application.Features.Companies.DTOs;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using Modules.CRM.Domain.ValueObjects;

namespace Modules.CRM.Application.Features.Companies.Commands;

public record CreateCompanyCommand(
    string Name,
    string Industry,
    string Website,
    string Phone,
    string Email,
    string AddressText,
    string Type = "LEAD"
) : ICommand<CompanyDto>;

public class CreateCompanyCommandHandler : ICommandHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCompanyCommandHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompanyDto> HandleAsync(CreateCompanyCommand command, CancellationToken cancellationToken = default)
    {
        var address = new Address(command.AddressText);
        var company = Company.Create(
            command.Name,
            command.Industry,
            command.Website,
            command.Phone,
            command.Email,
            address,
            string.IsNullOrWhiteSpace(command.Type) ? "LEAD" : command.Type
        );

        await _companyRepository.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return company.ToDto();
    }
}
