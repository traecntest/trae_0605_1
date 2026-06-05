using System.Collections.Concurrent;
using UTM.Communication.Protocols;
using Microsoft.Extensions.Logging;

namespace UTM.Communication.Services;

public class DroneCommunicationManager : IDroneCommunicationManager
{
    private readonly ConcurrentDictionary<byte, ConcurrentQueue<MavLinkMessage>> _messageQueues = new();
    private readonly ConcurrentDictionary<byte, DateTime> _lastHeartbeat = new();
    private readonly ILogger<DroneCommunicationManager> _logger;

    public DroneCommunicationManager(ILogger<DroneCommunicationManager> logger)
    {
        _logger = logger;
    }

    public ValueTask SendAsync(byte systemId, MavLinkMessage message, CancellationToken ct = default)
    {
        var queue = _messageQueues.GetOrAdd(systemId, _ => new ConcurrentQueue<MavLinkMessage>());
        queue.Enqueue(message);
        _logger.LogDebug("消息已入队: SystemId={SystemId}, MessageId={MessageId}", systemId, message.MessageType);
        return ValueTask.CompletedTask;
    }

    public ValueTask<MavLinkMessage?> ReceiveAsync(byte systemId, TimeSpan timeout, CancellationToken ct = default)
    {
        if (_messageQueues.TryGetValue(systemId, out var queue) && queue.TryDequeue(out var message))
        {
            return ValueTask.FromResult<MavLinkMessage?>(message);
        }

        return ValueTask.FromResult<MavLinkMessage?>(null);
    }

    public ValueTask BroadcastAsync(MavLinkMessage message, CancellationToken ct = default)
    {
        foreach (var systemId in _messageQueues.Keys)
        {
            var queue = _messageQueues.GetOrAdd(systemId, _ => new ConcurrentQueue<MavLinkMessage>());
            queue.Enqueue(message);
        }
        _logger.LogDebug("广播消息: MessageId={MessageId}, 目标数量={Count}", message.MessageType, _messageQueues.Count);
        return ValueTask.CompletedTask;
    }

    public IReadOnlyCollection<byte> GetConnectedSystems()
    {
        return _messageQueues.Keys.ToList();
    }

    public void UpdateHeartbeat(byte systemId)
    {
        _lastHeartbeat[systemId] = DateTime.UtcNow;
    }

    public bool IsSystemAlive(byte systemId, TimeSpan timeout)
    {
        if (_lastHeartbeat.TryGetValue(systemId, out var lastTime))
        {
            return (DateTime.UtcNow - lastTime) < timeout;
        }
        return false;
    }
}
