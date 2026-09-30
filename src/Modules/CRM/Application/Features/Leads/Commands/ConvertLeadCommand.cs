using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Leads.DTOs;
using Modules.CRM.Domain.Repositories;

namespace Modules.CRM.Application.Features.Leads.Commands;

public record ConvertLeadCommand(Guid LeadId) : ICommand<LeadConversionResultDto>;

public class ConvertLeadCommandHandler : ICommandHandler<ConvertLeadCommand, LeadConversionResultDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly ICustomerService _customerService;
    private readonly IUnitOfWork _unitOfWork;

    public ConvertLeadCommandHandler(
        ILeadRepository leadRepository,
        ICustomerService customerService,
        IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _customerService = customerService;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadConversionResultDto> HandleAsync(ConvertLeadCommand command, CancellationToken cancellationToken = default)
    {
        var lead = await _leadRepository.GetByIdAsync(command.LeadId, cancellationToken);
        if (lead == null || lead.IsDeleted)
        {
            throw new EntityNotFoundException("Lead", command.LeadId);
        }

        if (lead.Status == "CONVERTED" || lead.CustomerId != null)
        {
            throw new BusinessRuleException("Lead has already been converted.");
        }

        if (!lead.CanConvert())
        {
            throw new BusinessRuleException($"Lead in status '{lead.Status}' cannot be converted. Only QUALIFIED, PROPOSAL, or WON leads can be converted.");
        }

        var customerId = await _customerService.GetOrCreateCustomerForCompanyAsync(lead.CompanyId, cancellationToken);

        lead.ConvertToCustomer(customerId);

        _leadRepository.Update(lead);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LeadConversionResultDto
        {
            LeadId = lead.Id,
            CustomerId = customerId,
            CompanyId = lead.CompanyId,
            ContactId = lead.ContactId,
            ConvertedAt = lead.ConvertedAt ?? DateTime.UtcNow,
            Status = lead.Status
        };
    }
}
