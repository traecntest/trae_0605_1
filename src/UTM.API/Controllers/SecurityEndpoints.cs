using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using UTM.Security.Authentication;
using UTM.Security.Audit;

namespace UTM.API.Controllers;

public static class SecurityEndpoints
{
    public static void MapSecurityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/security").WithTags("Security");

        group.MapPost("/authenticate", async (AuthRequest request, IAuthenticationService authService, CancellationToken ct) =>
        {
            var result = await authService.AuthenticateAsync(request.Username, request.Password, ct);
            return result.IsAuthenticated ? Results.Ok(result) : Results.Unauthorized();
        });

        group.MapGet("/validate-token", async (string token, IAuthenticationService authService, CancellationToken ct) =>
        {
            var result = await authService.ValidateTokenAsync(token, ct);
            return result.IsAuthenticated ? Results.Ok(result) : Results.Unauthorized();
        });

        group.MapGet("/audit", async (string? userId, string? action, int limit, IAuditService auditService, CancellationToken ct) =>
        {
            var query = new AuditQuery(userId, action, null, null, limit > 0 ? limit : 100);
            var entries = await auditService.QueryAsync(query, ct);
            return Results.Ok(entries);
        });
    }
}

public sealed record AuthRequest(string Username, string Password);
