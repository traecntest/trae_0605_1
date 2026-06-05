using Microsoft.Extensions.DependencyInjection;
using UTM.ConflictDetection.Services;

namespace UTM.ConflictDetection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConflictDetection(this IServiceCollection services)
    {
        services.AddSingleton<IConflictDetector, ConflictDetector>();
        services.AddSingleton<TrajectoryPredictor>();
        services.AddSingleton<ResolutionAdvisor>();
        return services;
    }
}
