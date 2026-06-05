namespace UTM.Security.Authorization;

public interface IAuthorizationService
{
    Task<bool> AuthorizeAsync(string userId, string resource, string action, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionsAsync(string userId, CancellationToken ct = default);
}

public class RoleBasedAuthorizationService : IAuthorizationService
{
    private static readonly Dictionary<string, HashSet<string>> RolePermissions = new()
    {
        ["Administrator"] = ["*"],
        ["Operator"] = ["flightplan:submit", "flightplan:cancel", "airspace:read", "monitoring:read"],
        ["Pilot"] = ["flightplan:submit", "flightplan:read", "monitoring:read:own"],
        ["Inspector"] = ["monitoring:read", "audit:read", "airspace:read"],
        ["Regulator"] = ["monitoring:read", "flightplan:read", "airspace:read", "audit:read"]
    };

    public Task<bool> AuthorizeAsync(string userId, string resource, string action, CancellationToken ct = default)
    {
        // 简化实现：实际应从数据库查询用户角色
        var permissions = GetPermissionsForRole("Operator");
        var required = $"{resource}:{action}";
        return Task.FromResult(permissions.Contains(required) || permissions.Contains("*"));
    }

    public Task<IReadOnlyList<string>> GetPermissionsAsync(string userId, CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<string>>(GetPermissionsForRole("Operator").ToList());
    }

    private static HashSet<string> GetPermissionsForRole(string role)
    {
        return RolePermissions.TryGetValue(role, out var perms) ? perms : [];
    }
}
