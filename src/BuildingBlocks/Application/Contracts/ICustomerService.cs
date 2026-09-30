namespace BuildingBlocks.Application.Contracts;

public interface ICustomerService
{
    Task<Guid> GetOrCreateCustomerForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> CustomerExistsForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
}
