namespace BuildingBlocks.Application.Contracts;

public interface ICustomerService
{
    Task<Guid> GetOrCreateCustomerForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> CustomerExistsForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<bool> CustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default);
}
