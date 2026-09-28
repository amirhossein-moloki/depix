using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Companies.Commands;

public record ArchiveCompanyCommand(Guid Id, Guid? ArchivedBy = null) : ICommand;

public class ArchiveCompanyCommandHandler : ICommandHandler<ArchiveCompanyCommand>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveCompanyCommandHandler(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ArchiveCompanyCommand command, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(command.Id, cancellationToken);
        if (company == null)
        {
            throw new EntityNotFoundException(nameof(Company), command.Id);
        }

        company.SoftDelete(command.ArchivedBy);
        _companyRepository.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
