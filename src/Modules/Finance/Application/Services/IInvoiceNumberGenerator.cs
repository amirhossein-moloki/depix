namespace Modules.Finance.Application.Services;

public interface IInvoiceNumberGenerator
{
    Task<string> GenerateInvoiceNumberAsync(CancellationToken cancellationToken = default);
}
