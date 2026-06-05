using Microsoft.Extensions.DependencyInjection;
using UTM.Security.Authentication;
using UTM.Security.Authorization;
using UTM.Security.Audit;
using UTM.Security.Encryption;

namespace UTM.Security;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IAuthenticationService, InMemoryAuthenticationService>();
        services.AddSingleton<IAuthorizationService, RoleBasedAuthorizationService>();
        services.AddSingleton<IAuditService, InMemoryAuditService>();
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        return services;
    }
}
