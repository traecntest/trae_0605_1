using Microsoft.Extensions.DependencyInjection;
using UTM.Monitoring.Services;
using UTM.Monitoring.Spatial;

namespace UTM.Monitoring;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMonitoring(this IServiceCollection services)
    {
        services.AddSingleton<ITrajectoryTracker, TrajectoryTracker>();
        services.AddSingleton<AircraftStateProcessor>();
        services.AddSingleton<RealTimeDataStream>();
        services.AddSingleton<GeoFenceChecker>();
        return services;
    }
}
