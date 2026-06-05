namespace UTM.Security.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken ct = default);
    Task<AuthenticationResult> ValidateTokenAsync(string token, CancellationToken ct = default);
    Task<string> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}

public sealed record AuthenticationResult(bool IsAuthenticated, string? UserId, string? Token, string? Role, DateTime? ExpiresAt);
