using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;

namespace UTM.Security.Authentication;

public class InMemoryAuthenticationService : IAuthenticationService
{
    private readonly ConcurrentDictionary<string, UserCredential> _users = new();
    private readonly ConcurrentDictionary<string, TokenInfo> _tokens = new();
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromHours(1);

    public InMemoryAuthenticationService()
    {
        // 添加测试用户
        _users["admin"] = new UserCredential("admin", HashPassword("admin123"), "Administrator");
        _users["operator1"] = new UserCredential("operator1", HashPassword("op123"), "Operator");
    }

    public Task<AuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        if (_users.TryGetValue(username, out var cred) && cred.PasswordHash == HashPassword(password))
        {
            var token = GenerateToken();
            var expiresAt = DateTime.UtcNow.Add(_tokenLifetime);
            _tokens[token] = new TokenInfo(cred.UserId, cred.Role, expiresAt);
            return Task.FromResult(new AuthenticationResult(true, cred.UserId, token, cred.Role, expiresAt));
        }
        return Task.FromResult(new AuthenticationResult(false, null, null, null, null));
    }

    public Task<AuthenticationResult> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        if (_tokens.TryGetValue(token, out var info) && info.ExpiresAt > DateTime.UtcNow)
        {
            return Task.FromResult(new AuthenticationResult(true, info.UserId, token, info.Role, info.ExpiresAt));
        }
        return Task.FromResult(new AuthenticationResult(false, null, null, null, null));
    }

    public Task<string> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var newToken = GenerateToken();
        var expiresAt = DateTime.UtcNow.Add(_tokenLifetime);
        // 简化实现：实际应从refreshToken解析userId
        _tokens[newToken] = new TokenInfo("user", "Operator", expiresAt);
        return Task.FromResult(newToken);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private sealed record UserCredential(string UserId, string PasswordHash, string Role);
    private sealed record TokenInfo(string UserId, string Role, DateTime ExpiresAt);
}
