using System.Collections.Concurrent;

namespace UTM.Security.Audit;

public class InMemoryAuditService : IAuditService
{
    private readonly ConcurrentQueue<AuditEntry> _auditLog = new();
    private readonly int _maxEntries = 10000;

    public Task LogAsync(AuditEntry entry, CancellationToken ct = default)
    {
        _auditLog.Enqueue(entry);
        
        // 限制内存大小
        while (_auditLog.Count > _maxEntries)
        {
            _auditLog.TryDequeue(out _);
        }
        
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditEntry>> QueryAsync(AuditQuery query, CancellationToken ct = default)
    {
        var results = _auditLog.AsEnumerable();

        if (!string.IsNullOrEmpty(query.UserId))
            results = results.Where(e => e.UserId == query.UserId);

        if (!string.IsNullOrEmpty(query.Action))
            results = results.Where(e => e.Action == query.Action);

        if (query.StartTime.HasValue)
            results = results.Where(e => e.Timestamp >= query.StartTime.Value);

        if (query.EndTime.HasValue)
            results = results.Where(e => e.Timestamp <= query.EndTime.Value);

        return Task.FromResult<IReadOnlyList<AuditEntry>>(
            results.OrderByDescending(e => e.Timestamp).Take(query.Limit).ToList()
        );
    }
}
