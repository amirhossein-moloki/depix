using BuildingBlocks.Application.CQRS;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Modules.Finance.Application.DTOs;
using Modules.Finance.Application.Features.Invoices.Commands;
using Modules.Finance.Application.Features.Invoices.Queries;
using Modules.Finance.Application.Validators;

namespace Modules.Finance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        // Invoice Command Handlers
        services.AddScoped<ICommandHandler<CreateInvoiceCommand, InvoiceDto>, CreateInvoiceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateInvoiceCommand, InvoiceDto>, UpdateInvoiceCommandHandler>();
        services.AddScoped<ICommandHandler<AddInvoiceItemCommand, InvoiceDto>, AddInvoiceItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateInvoiceItemCommand, InvoiceDto>, UpdateInvoiceItemCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveInvoiceItemCommand, InvoiceDto>, RemoveInvoiceItemCommandHandler>();
        services.AddScoped<ICommandHandler<IssueInvoiceCommand, InvoiceDto>, IssueInvoiceCommandHandler>();
        services.AddScoped<ICommandHandler<CancelInvoiceCommand, InvoiceDto>, CancelInvoiceCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteInvoiceCommand, bool>, DeleteInvoiceCommandHandler>();
        services.AddScoped<ICommandHandler<RecordPaymentCommand, PaymentDto>, RecordPaymentCommandHandler>();

        // Invoice Query Handlers
        services.AddScoped<IQueryHandler<GetInvoiceByIdQuery, InvoiceDto>, GetInvoiceByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetInvoicesQuery, PagedResult<InvoiceListItemDto>>, GetInvoicesQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomerInvoicesQuery, List<InvoiceListItemDto>>, GetCustomerInvoicesQueryHandler>();
        services.AddScoped<IQueryHandler<GetProjectInvoicesQuery, List<InvoiceListItemDto>>, GetProjectInvoicesQueryHandler>();
        services.AddScoped<IQueryHandler<GetInvoicePaymentsQuery, List<PaymentDto>>, GetInvoicePaymentsQueryHandler>();

        // Invoice Validators
        services.AddScoped<IValidator<CreateInvoiceCommand>, CreateInvoiceCommandValidator>();
        services.AddScoped<IValidator<UpdateInvoiceCommand>, UpdateInvoiceCommandValidator>();
        services.AddScoped<IValidator<AddInvoiceItemCommand>, AddInvoiceItemCommandValidator>();
        services.AddScoped<IValidator<UpdateInvoiceItemCommand>, UpdateInvoiceItemCommandValidator>();
        services.AddScoped<IValidator<RecordPaymentCommand>, RecordPaymentCommandValidator>();

        return services;
    }
}
