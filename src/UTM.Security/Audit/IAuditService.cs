namespace UTM.Security.Audit;

public interface IAuditService
{
    Task LogAsync(AuditEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<AuditEntry>> QueryAsync(AuditQuery query, CancellationToken ct = default);
}

public sealed record AuditEntry(
    string UserId,
    string Action,
    string Resource,
    string? ResourceId,
    DateTime Timestamp,
    string? Details,
    string? IpAddress
);

public sealed record AuditQuery(
    string? UserId,
    string? Action,
    DateTime? StartTime,
    DateTime? EndTime,
    int Limit = 100
);
