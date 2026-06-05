using Microsoft.Extensions.DependencyInjection;
using UTM.Communication.Services;

namespace UTM.Communication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommunication(this IServiceCollection services)
    {
        services.AddSingleton<IDroneCommunicationManager, DroneCommunicationManager>();
        services.AddSingleton<IMessageRouter, MessageRouter>();
        return services;
    }
}
