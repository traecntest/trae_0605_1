using Microsoft.Extensions.DependencyInjection;
using UTM.Airspace.Repositories;
using UTM.Airspace.Services;
using UTM.Core.Interfaces;

namespace UTM.Airspace.DependencyInjection;

public static class AirspaceServiceCollectionExtensions
{
    public static IServiceCollection AddAirspaceServices(this IServiceCollection services)
    {
        services.AddSingleton<IFlightPlanRepository, InMemoryFlightPlanRepository>();
        services.AddSingleton<IFlightPlanService, FlightPlanService>();
        return services;
    }
}
