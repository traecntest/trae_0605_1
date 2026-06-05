using System.Collections.Concurrent;
using UTM.Communication.Protocols;
using Microsoft.Extensions.Logging;

namespace UTM.Communication.Services;

public class MessageRouter : IMessageRouter
{
    private readonly ConcurrentDictionary<MavLinkMessageType, Func<MavLinkMessage, Task>> _handlers = new();
    private readonly ILogger<MessageRouter> _logger;

    public MessageRouter(ILogger<MessageRouter> logger)
    {
        _logger = logger;
    }

    public void RegisterHandler(MavLinkMessageType messageType, Func<MavLinkMessage, Task> handler)
    {
        _handlers[messageType] = handler;
        _logger.LogInformation("注册消息处理器: MessageType={MessageType}", messageType);
    }

    public void UnregisterHandler(MavLinkMessageType messageType)
    {
        _handlers.TryRemove(messageType, out _);
        _logger.LogInformation("注销消息处理器: MessageType={MessageType}", messageType);
    }

    public async ValueTask RouteAsync(MavLinkMessage message, CancellationToken ct = default)
    {
        if (_handlers.TryGetValue(message.MessageType, out var handler))
        {
            try
            {
                await handler(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "消息处理失败: MessageType={MessageType}", message.MessageType);
            }
        }
        else
        {
            _logger.LogWarning("未找到消息处理器: MessageType={MessageType}", message.MessageType);
        }
    }
}
