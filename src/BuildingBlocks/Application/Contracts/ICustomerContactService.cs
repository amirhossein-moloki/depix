namespace BuildingBlocks.Application.Contracts;

public record CustomerContactContractDto(
    Guid Id,
    Guid CompanyId,
    string Name,
    string? Position,
    string? Phone,
    string? Email,
    bool IsDecisionMaker,
    string? InfluenceLevel
);

public interface ICustomerContactService
{
    Task<List<CustomerContactContractDto>> GetContactsByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<CustomerContactContractDto?> GetContactByIdAsync(Guid contactId, CancellationToken cancellationToken = default);
}
