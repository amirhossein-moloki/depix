using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Exceptions;
using Modules.Finance.Application.DTOs;
using Modules.Finance.Application.Mappings;
using Modules.Finance.Domain.Repositories;

namespace Modules.Finance.Application.Features.Invoices.Queries;

public record GetInvoiceByIdQuery(Guid InvoiceId) : IQuery<InvoiceDto>;

public record GetInvoicesQuery(InvoiceFilterParams FilterParams) : IQuery<PagedResult<InvoiceListItemDto>>;

public record GetCustomerInvoicesQuery(Guid CustomerId) : IQuery<List<InvoiceListItemDto>>;

public record GetProjectInvoicesQuery(Guid ProjectId) : IQuery<List<InvoiceListItemDto>>;

public record GetInvoicePaymentsQuery(Guid InvoiceId) : IQuery<List<PaymentDto>>;

public class GetInvoiceByIdQueryHandler : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<InvoiceDto> HandleAsync(GetInvoiceByIdQuery query, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(query.InvoiceId, cancellationToken)
            ?? throw new EntityNotFoundException("Invoice", query.InvoiceId);

        return invoice.ToDto();
    }
}

public class GetInvoicesQueryHandler : IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItemDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<PagedResult<InvoiceListItemDto>> HandleAsync(GetInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _invoiceRepository.GetFilteredAsync(query.FilterParams, cancellationToken);
        var dtos = items.Select(i => i.ToListItemDto()).ToList();

        return new PagedResult<InvoiceListItemDto>(
            dtos,
            totalCount,
            query.FilterParams.Page,
            query.FilterParams.PageSize);
    }
}

public class GetCustomerInvoicesQueryHandler : IQueryHandler<GetCustomerInvoicesQuery, List<InvoiceListItemDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetCustomerInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<List<InvoiceListItemDto>> HandleAsync(GetCustomerInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);
        return invoices.Select(i => i.ToListItemDto()).ToList();
    }
}

public class GetProjectInvoicesQueryHandler : IQueryHandler<GetProjectInvoicesQuery, List<InvoiceListItemDto>>
{
    private readonly IInvoiceRepository _invoiceRepository;

    public GetProjectInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<List<InvoiceListItemDto>> HandleAsync(GetProjectInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetByProjectIdAsync(query.ProjectId, cancellationToken);
        return invoices.Select(i => i.ToListItemDto()).ToList();
    }
}

public class GetInvoicePaymentsQueryHandler : IQueryHandler<GetInvoicePaymentsQuery, List<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetInvoicePaymentsQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<List<PaymentDto>> HandleAsync(GetInvoicePaymentsQuery query, CancellationToken cancellationToken = default)
    {
        var payments = await _paymentRepository.GetByInvoiceIdAsync(query.InvoiceId, cancellationToken);
        return payments.Select(p => p.ToDto()).ToList();
    }
}
