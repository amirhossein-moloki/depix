using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Common.Responses;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Application.Contracts;
using Modules.Customer.Application.Commands;
using Modules.Customer.Application.DTOs;
using Modules.Customer.Application.Queries;
using Modules.Customer.Application.Validators;

namespace Modules.Customer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerApplication(this IServiceCollection services)
    {
        // Handlers
        services.AddScoped<ICommandHandler<CreateCustomerCommand, CustomerDto>, CreateCustomerCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCustomerCommand, CustomerDto>, UpdateCustomerCommandHandler>();
        services.AddScoped<ICommandHandler<AssignCustomerCommand, CustomerDto>, AssignCustomerCommandHandler>();
        services.AddScoped<ICommandHandler<ArchiveCustomerCommand>, ArchiveCustomerCommandHandler>();
        services.AddScoped<ICommandHandler<ReactivateCustomerCommand, CustomerDto>, ReactivateCustomerCommandHandler>();

        services.AddScoped<IQueryHandler<GetCustomerByIdQuery, CustomerDetailDto>, GetCustomerByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomersQuery, PagedResult<CustomerListItemDto>>, GetCustomersQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomerByCompanyQuery, CustomerDetailDto>, GetCustomerByCompanyQueryHandler>();
        services.AddScoped<IQueryHandler<GetCustomerContactsQuery, List<CustomerContactContractDto>>, GetCustomerContactsQueryHandler>();

        // Validators
        services.AddScoped<IValidator<CreateCustomerCommand>, CreateCustomerCommandValidator>();
        services.AddScoped<IValidator<UpdateCustomerCommand>, UpdateCustomerCommandValidator>();
        services.AddScoped<IValidator<AssignCustomerCommand>, AssignCustomerCommandValidator>();

        return services;
    }
}
