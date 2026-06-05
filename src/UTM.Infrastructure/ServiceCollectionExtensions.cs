using Microsoft.Extensions.DependencyInjection;
using UTM.Core.Interfaces;
using UTM.Infrastructure.Persistence;

namespace UTM.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAircraftRepository, InMemoryAircraftRepository>();
        return services;
    }
}
