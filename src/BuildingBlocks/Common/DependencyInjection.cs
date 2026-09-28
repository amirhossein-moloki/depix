using BuildingBlocks.Common.Extensions;
using BuildingBlocks.SharedKernel.Clock;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddBuildingBlocksCommon(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddUnifiedApiResponseAndExceptionHandling();
        return services;
    }
}
